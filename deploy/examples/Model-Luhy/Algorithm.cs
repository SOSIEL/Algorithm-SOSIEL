using System;
using System.Collections.Generic;
using System.Linq;
using ModelLuhy.Configuration;
using ModelLuhy.Helpers;
using ModelLuhy.Output;
using SOSIEL.Algorithm;
using SOSIEL.Configuration;
using SOSIEL.Entities;
using SOSIEL.Exceptions;
using SOSIEL.Processes;
using SOSIEL.Helpers;

namespace ModelLuhy
{
    public sealed class Algorithm : SosielAlgorithm<Site>, IAlgorithm<AlgorithmModel>
    {
        public string Name { get { return "SOSIEL"; } }

        string _outputFolder;

        ConfigurationModel _configuration;

        public static ProcessesConfiguration GetProcessConfiguration()
        {
            return new ProcessesConfiguration
            {
                ActionTakingEnabled = true,
                AnticipatoryLearningEnabled = true,
                DecisionOptionSelectionEnabled = true,
                DecisionOptionSelectionPart2Enabled = true,
                SocialLearningEnabled = true,
                CounterfactualThinkingEnabled = true,
                InnovationEnabled = true,
                ReproductionEnabled = false,
                AgentRandomizationEnabled = true,
                AgentsDeactivationEnabled = false,
                AlgorithmStopIfAllAgentsSelectDoNothing = false
            };
        }

        public Algorithm(ConfigurationModel configuration) : base(1, GetProcessConfiguration())
        {
            _configuration = configuration;
        }

        public AlgorithmModel Run(AlgorithmModel model)
        {
            _outputFolder = model.OutputFolder;

            Initialize(model);

            var sites = new Site[] { DefaultDataSet };

            Enumerable.Range(1, _configuration.AlgorithmConfiguration.NumberOfIterations).ForEach(iteration =>
            {
                Console.WriteLine((string)"Starting {0} iteration", (object)iteration);

                RunSosiel(sites);
            });

            return model;
        }

        /// <summary>
        /// Executes algorithm initialization
        /// </summary>
        public void Initialize(AlgorithmModel model)
        {
            InitializeAgents();

            InitializeProbabilities();

            if (_configuration.AlgorithmConfiguration.UseDimographicProcesses)
            {
                UseDemographic();
            }

            AfterInitialization();
        }

        protected override void UseDemographic()
        {
            base.UseDemographic();

            demographic = new Demographic<Site>(_configuration.AlgorithmConfiguration.DemographicConfiguration,
                probabilities.GetProbabilityTable<int>(AlgorithmProbabilityTables.BirthProbabilityTable),
                probabilities.GetProbabilityTable<int>(AlgorithmProbabilityTables.DeathProbabilityTable));
        }

        /// <inheritdoc />
        protected override void InitializeAgents()
        {
            var agents = new List<IAgent>();

            Dictionary<string, AgentArchetype> agentArchetypes = _configuration.AgentConfiguration;

            if (agentArchetypes.Count == 0)
            {
                throw new SosielAlgorithmException("Agent archetypes were not defined. See configuration file");
            }

            InitialStateConfiguration initialState = _configuration.InitialState;

            var networks = new Dictionary<string, List<SOSIEL.Entities.Agent>>();

            //create agents, groupby is used for saving agents numeration, e.g. FE1, HM1. HM2 etc
            initialState.AgentsState.GroupBy(state => state.ArchetypeOfAgent).ForEach((agentStateGroup) =>
            {
                AgentArchetype archetype = agentArchetypes[agentStateGroup.Key];
                var mentalProto = archetype.MentalProto;
                int index = 1;

                agentStateGroup.ForEach((agentState) =>
                {
                    for (int i = 0; i < agentState.NumberOfAgents; i++)
                    {
                        SOSIEL.Entities.Agent agent = Agent.CreateAgent(agentState, archetype);
                        agent.SetId(index);

                        agents.Add(agent);

                        networks.AddToDictionary((string)agent[AlgorithmVariables.Household], agent);
                        networks.AddToDictionary((string)agent[AlgorithmVariables.NuclearFamily], agent);

                        if (agent.ContainsVariable(AlgorithmVariables.ExternalRelations))
                        {
                            var externals = (string)agent[AlgorithmVariables.ExternalRelations];

                            foreach (var en in externals.Split(';'))
                            {
                                networks.AddToDictionary(en, agent);
                            }
                        }

                        //household and extended family are the same at the beginning
                        agent[AlgorithmVariables.ExtendedFamily] = new List<string>() { (string)agent[AlgorithmVariables.Household] };

                        index++;
                    }
                });
            });

            //convert temp networks to list of connetcted agents
            networks.ForEach(kvp =>
            {
                var connectedAgents = kvp.Value;

                connectedAgents.ForEach(agent =>
                {
                    agent.ConnectedAgents.AddRange(connectedAgents.Where(a => a != agent).Except(agent.ConnectedAgents));
                });

            });


            agentList = new AgentList(agents, agentArchetypes.Select(kvp => kvp.Value).ToList());
        }

        private void InitializeProbabilities()
        {
            var probabilitiesList = new Probabilities();

            foreach (var probabilityElementConfiguration in _configuration.AlgorithmConfiguration.ProbabilitiesConfiguration)
            {
                var variableType = VariableTypeHelper.ConvertStringToType(probabilityElementConfiguration.VariableType);
                var parseTableMethod = ReflectionHelper.GetGenerecMethod(variableType, typeof(ProbabilityTableParser), "Parse");

                dynamic table = parseTableMethod.Invoke(null, new object[] { probabilityElementConfiguration.FilePath, probabilityElementConfiguration.WithHeader });

                var addToListMethod =
                    ReflectionHelper.GetGenerecMethod(variableType, typeof(Probabilities), "AddProbabilityTable");

                addToListMethod.Invoke(probabilitiesList, new object[] { probabilityElementConfiguration.Variable, table });
            }

            probabilities = probabilitiesList;
        }

        protected override void AfterInitialization()
        {
            base.AfterInitialization();

            var hmAgents = agentList.GetAgentsWithPrefix("HM");

            hmAgents.ForEach(agent =>
            {
                agent[AlgorithmVariables.AgentIncome] = 0d;
            });
        }

        /// <inheritdoc />
        protected override Dictionary<IAgent, AgentState<Site>> InitializeFirstIterationState()
        {
            var states = new Dictionary<IAgent, AgentState<Site>>();

            agentList.Agents.ForEach(agent =>
            {
                //creates empty agent state
                AgentState<Site> agentState = AgentState<Site>.Create(agent.Archetype.IsDataSetOriented);

                //copy generated goal importance
                agent.InitialGoalStates.ForEach(kvp =>
                {
                    var goalState = kvp.Value;
                    goalState.Value = agent[kvp.Key.ReferenceVariable];

                    agentState.GoalsState[kvp.Key] = goalState;
                });

                states.Add(agent, agentState);
            });

            return states;
        }

        protected override void Maintenance()
        {
            base.Maintenance();

            var hmAgents = agentList.Agents.Where(a => a.Archetype.NamePrefix == "HM");

            hmAgents.ForEach(agent =>
            {
                //increase household members age

                if ((bool)agent[AlgorithmVariables.IsActive])
                {
                    agent[AlgorithmVariables.Age] += 1;
                }
                else
                {
                    agent[AlgorithmVariables.AgentIncome] = 0;
                    agent[AlgorithmVariables.AgentExpenses] = 0;
                    agent[AlgorithmVariables.HouseholdSavings] = 0;
                }
            });


        }

        protected override void PostIterationCalculations(int iteration)
        {
            base.PostIterationCalculations(iteration);

            //----
            //calculate household values (income, expenses, savings) for each agent in specific household
            var hmAgents = agentList.GetAgentsWithPrefix("HM");

            hmAgents.GroupBy(agent => agent[SosielVariables.Household])
                .ForEach(householdAgents =>
                {
                    double householdIncome =
                        householdAgents.Sum(agent => (double)agent[AlgorithmVariables.AgentIncome]);
                    double householdExpenses =
                        householdAgents.Sum(agent => (double)agent[AlgorithmVariables.AgentExpenses]);
                    double iterationHouseholdSavings = householdIncome - householdExpenses;
                    double householdSavings = householdAgents.Where(agent => agent.ContainsVariable(AlgorithmVariables.HouseholdSavings))
                                                  .Select(agent => (double)agent[AlgorithmVariables.HouseholdSavings]).FirstOrDefault() + iterationHouseholdSavings;

                    householdAgents.ForEach(agent =>
                    {
                        agent[AlgorithmVariables.HouseholdIncome] = householdIncome;
                        agent[AlgorithmVariables.HouseholdExpenses] = householdExpenses;
                        agent[AlgorithmVariables.HouseholdSavings] = householdSavings;
                    });
                });
        }

        /// <inheritdoc />
        protected override void PostIterationStatistic(int iteration)
        {
            base.PostIterationStatistic(iteration);

            var lastIteration = iterations.Last.Value;

            agentList.Agents.ForEach(agent =>
            {
                AgentState<Site> agentState;

                lastIteration.TryGetValue(agent, out agentState);

                var details = new AgentDetailsOutput
                {
                    Iteration = iteration,
                    AgentId = agent.Id,
                    Age = agent[AlgorithmVariables.Age],
                    IsAlive = agent[AlgorithmVariables.IsActive],
                    Income = agent[AlgorithmVariables.AgentIncome],
                    Expenses = agent[AlgorithmVariables.AgentExpenses],
                    Savings = agent[AlgorithmVariables.HouseholdSavings],
                    NumberOfDO = agent.AssignedDecisionOptions.Count,
                    ChosenDecisionOption = agentState != null ? string.Join("|", agentState.DecisionOptionsHistories[DefaultDataSet].Activated.Select(opt => opt.Id)) : string.Empty
                };

                CSVHelper.AppendTo(_outputFolder + string.Format(AgentDetailsOutput.FileName, agent.Id), details);
            });
        }
    }
}
