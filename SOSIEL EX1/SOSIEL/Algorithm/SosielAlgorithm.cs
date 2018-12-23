using System.Collections.Generic;
using System.Linq;
using SOSIEL.Configuration;
using SOSIEL.Entities;
using SOSIEL.Helpers;
using SOSIEL.Processes;

namespace SOSIEL.Algorithm
{
    public abstract class SosielAlgorithm<TSite> where TSite : new()
    {
        protected readonly TSite DefaultSite = new TSite();

        private int numberOfIterations;
        private int iterationCounter;
        private ProcessesConfiguration processConfiguration;

        protected int numberOfAgentsAfterInitialize;
        protected bool algorithmStoppage = false;
        protected AgentList agentList;
        protected LinkedList<Dictionary<IAgent, AgentState<TSite>>> iterations = new LinkedList<Dictionary<IAgent, AgentState<TSite>>>();
        protected Dictionary<IAgent, Goal[]> rankedGoals;

        protected Probabilities probabilities = new Probabilities();

        //processes
        protected GoalPrioritizing gp = new GoalPrioritizing();
        protected GoalSelecting gs = new GoalSelecting();
        protected AnticipatoryLearning<TSite> al = new AnticipatoryLearning<TSite>();
        protected CounterfactualThinking<TSite> ct = new CounterfactualThinking<TSite>();
        protected Innovation<TSite> innovation = new Innovation<TSite>();
        protected SocialLearning<TSite> sl = new SocialLearning<TSite>();
        protected Satisficing<TSite> satisficing = new Satisficing<TSite>();
        protected ActionTaking<TSite> at = new ActionTaking<TSite>();

        protected Demographic<TSite> demographic;


        public SosielAlgorithm(int numberOfIterations, ProcessesConfiguration processConfiguration)
        {
            this.numberOfIterations = numberOfIterations;
            this.processConfiguration = processConfiguration;

            iterationCounter = 0;
        }

        /// <summary>
        /// Executes agent initializing. It's the first initializing step. 
        /// </summary>
        protected abstract void InitializeAgents();


        /// <summary>
        /// Executes iteration state initializing. Executed after InitializeAgents.
        /// </summary>
        /// <returns></returns>
        protected abstract Dictionary<IAgent, AgentState<TSite>> InitializeFirstIterationState();

        protected virtual void UseDemographic()
        {
            processConfiguration.UseDemographicProcesses = true;
        }

        /// <summary>
        /// Executes last preparations before runs the algorithm. Executes after InitializeAgents and InitializeFirstIterationState.
        /// </summary>
        protected virtual void AfterInitialization() { }

        /// <summary>
        /// Executes in the end of the algorithm.
        /// </summary>
        protected virtual void AfterAlgorithmExecuted() { }


        /// <summary>
        /// Executes before any cognitive process is started.
        /// </summary>
        /// <param name="iteration"></param>
        protected virtual void PreIterationCalculations(int iteration) { }


        /// <summary>
        /// Executes after PreIterationCalculations
        /// </summary>
        /// <param name="iteration"></param>
        protected virtual void PreIterationStatistic(int iteration) { }


        /// <summary>
        /// Executes before action selection process
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="site"></param>
        protected virtual void BeforeActionSelection(IAgent agent, TSite site) { }


        /// <summary>
        /// Executes after action taking process
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="site"></param>
        protected virtual void AfterActionTaking(IAgent agent, TSite site) { }


        /// <summary>
        /// Befores the counterfactual thinking.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="site"></param>
        protected virtual void BeforeCounterfactualThinking(IAgent agent, TSite site) { }


        /// <summary>
        /// Executes after last cognitive process is finished
        /// </summary>
        /// <param name="iteration"></param>
        protected virtual void PostIterationCalculations(int iteration) { }

        /// <summary>
        /// Executes after PostIterationCalculations
        /// </summary>
        /// <param name="iteration"></param>
        protected virtual void PostIterationStatistic(int iteration) { }

        /// <summary>
        /// Executes agent deactivation logic.
        /// </summary>
        protected virtual void AgentsDeactivation() { }

        /// <summary>
        /// Executed after AgentsDeactivation.
        /// </summary>
        /// <param name="iteration"></param>
        protected virtual void AfterDeactivation(int iteration) { }



        /// <summary>
        /// Executes reproduction logic.
        /// </summary>
        /// <param name="minAgentNumber"></param>
        protected virtual void Reproduction(int minAgentNumber)
        {

        }

        /// <summary>
        /// Executes maintenance logic.
        /// </summary>
        protected virtual void Maintenance()
        {
            agentList.ActiveAgents.ForEach(a =>
            {
                //increment decision option activation freshness
                a.DecisionOptionActivationFreshness.Keys.ToList().ForEach(k =>
                {
                    a.DecisionOptionActivationFreshness[k] += 1;
                });
            });
        }


        /// <summary>
        /// Executes SOSIEL Algorithm
        /// </summary>
        /// <param name="activeSites"></param>
        protected void RunSosiel(ICollection<TSite> activeSites)
        {
            for (int i = 1; i <= numberOfIterations; i++)
            {
                iterationCounter++;

                PreIterationCalculations(iterationCounter);
                PreIterationStatistic(iterationCounter);

                Dictionary<IAgent, AgentState<TSite>> currentIteration;

                if (iterationCounter > 1)
                    currentIteration = iterations.AddLast(new Dictionary<IAgent, AgentState<TSite>>()).Value;
                else
                    currentIteration = iterations.AddLast(InitializeFirstIterationState()).Value;

                Dictionary<IAgent, AgentState<TSite>> priorIteration = iterations.Last.Previous?.Value;

                rankedGoals = new Dictionary<IAgent, Goal[]>(agentList.Agents.Count);

                IAgent[] orderedAgents = agentList.ActiveAgents.Randomize(processConfiguration.AgentRandomizationEnabled).ToArray();

                var agentGroups = orderedAgents.GroupBy(a => a[SosielVariables.AgentType]).OrderBy(group => group.Key).ToArray();

                orderedAgents.ForEach(a =>
                {
                    rankedGoals.Add(a, a.AssignedGoals.ToArray());

                    if (iterationCounter > 1)
                        currentIteration.Add(a, priorIteration[a].CreateForNextIteration());
                });

                if (processConfiguration.UseDemographicProcesses && iterationCounter > 1)
                {
                    demographic.ChangeDemographic(iterationCounter, currentIteration, agentList);
                }
                
                TSite[] orderedSites = activeSites.Randomize().ToArray();

                TSite[] notSiteOriented = new TSite[] { DefaultSite };

                if (iterationCounter == 1)
                {
                    foreach (var agentGroup in agentGroups)
                    {
                        foreach (IAgent agent in agentGroup)
                        {
                            rankedGoals[agent] = gs.SortByImportance(agent, currentIteration[agent].GoalsState)
                                .ToArray();
                        }
                    }
                }

                if (processConfiguration.AnticipatoryLearningEnabled && iterationCounter > 1)
                {
                    //1st round: AL, CT, IR
                    foreach (var agentGroup in agentGroups)
                    {
                        foreach (IAgent agent in agentGroup)
                        {
                            //anticipatory learning process
                            al.Execute(agent, iterations.Last);


                            var agentGoalState = currentIteration[agent].GoalsState;
                            //goal prioritizing
                            gp.Prioritize(agent, agentGoalState);

                            //goal selecting
                            rankedGoals[agent] = gs.SortByImportance(agent, agentGoalState).ToArray();

                            if (processConfiguration.CounterfactualThinkingEnabled)
                            {
                                if (rankedGoals[agent].Any(g => currentIteration[agent].GoalsState.Any(kvp => kvp.Value.Confidence == false)))
                                {
                                    foreach (TSite site in agent.Prototype.IsSiteOriented ? orderedSites : notSiteOriented)
                                    {
                                        BeforeCounterfactualThinking(agent, site);

                                        foreach (var set in agent.AssignedDecisionOptions.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                                        {
                                            //optimization
                                            Goal selectedGoal = rankedGoals[agent].First(g => set.Key.AssociatedWith.Contains(g));

                                            GoalState selectedGoalState = currentIteration[agent].GoalsState[selectedGoal];

                                            if (selectedGoalState.Confidence == false)
                                            {
                                                foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                                {
                                                    if (layer.Key.LayerConfiguration.Modifiable || (!layer.Key.LayerConfiguration.Modifiable && layer.Any(r => r.IsModifiable)))
                                                    {
                                                        //looking for matched decision option in prior period
                                                        DecisionOption[] matchedDecisionOptions = priorIteration[agent].DecisionOptionsHistories[site]
                                                                .Matched.Where(h => h.Layer == layer.Key).ToArray();

                                                        bool? CTResult = null;

                                                        //counterfactual thinking process
                                                        if (matchedDecisionOptions.Length >= 2)
                                                            CTResult = ct.Execute(agent, iterations.Last, selectedGoal, matchedDecisionOptions, layer.Key, site);


                                                        if (processConfiguration.InnovationEnabled)
                                                        {
                                                            //innovation process
                                                            if (CTResult == false || matchedDecisionOptions.Length < 2)
                                                                innovation.Execute(agent, iterations.Last, selectedGoal, layer.Key, site, probabilities);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (processConfiguration.SocialLearningEnabled && iterationCounter > 1)
                {
                    //2nd round: SL
                    foreach (var agentGroup in agentGroups)
                    {

                        foreach (IAgent agent in agentGroup)
                        {
                            foreach (var set in agent.AssignedDecisionOptions.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                            {
                                foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                {
                                    //social learning process
                                    sl.ExecuteLearning(agent, iterations.Last, layer.Key);
                                }
                            }
                        }
                    }

                }

                if (processConfiguration.DecisionOptionSelectionEnabled)
                {
                    //AS part I
                    foreach (var agentGroup in agentGroups)
                    {
                        foreach (IAgent agent in agentGroup)
                        {
                            foreach (TSite site in agent.Prototype.IsSiteOriented ? orderedSites : notSiteOriented)
                            {
                                foreach (var set in agent.AssignedDecisionOptions.GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                                {
                                    foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                    {
                                        BeforeActionSelection(agent, site);

                                        //satisficing
                                        satisficing.ExecutePartI(agent, iterations.Last, rankedGoals, layer.ToArray(), site);
                                    }
                                }
                            }
                        }
                    }


                    if (processConfiguration.DecisionOptionSelectionPart2Enabled && iterationCounter > 1)
                    {
                        //4th round: AS part II
                        foreach (var agentGroup in agentGroups)
                        {
                            foreach (IAgent agent in agentGroup)
                            {
                                foreach (TSite site in agent.Prototype.IsSiteOriented ? orderedSites : notSiteOriented)
                                {
                                    foreach (var set in agent.AssignedDecisionOptions.GroupBy(r => r.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                                    {
                                        foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                        {
                                            BeforeActionSelection(agent, site);

                                            //action selection process part II
                                            satisficing.ExecutePartII(agent, iterations.Last, rankedGoals, layer.ToArray(), site);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (processConfiguration.ActionTakingEnabled)
                {
                    //5th round: TA
                    foreach (var agentGroup in agentGroups)
                    {
                        foreach (IAgent agent in agentGroup)
                        {
                            foreach (TSite site in agent.Prototype.IsSiteOriented ? orderedSites : notSiteOriented)
                            {
                                at.Execute(agent, currentIteration[agent], site);

                                AfterActionTaking(agent, site);
                            }
                        }
                    }
                }

                if (processConfiguration.AlgorithmStopIfAllAgentsSelectDoNothing && iterationCounter > 1)
                {
                    if (!currentIteration.SelectMany(kvp => kvp.Value.DecisionOptionsHistories.Values.SelectMany(rh => rh.Activated)).Any())
                    {
                        algorithmStoppage = true;
                    }
                }

                PostIterationCalculations(iterationCounter);

                PostIterationStatistic(iterationCounter);

                if (processConfiguration.AgentsDeactivationEnabled && iterationCounter > 1)
                {
                    AgentsDeactivation();
                }

                AfterDeactivation(iterationCounter);

                if (processConfiguration.ReproductionEnabled && iterationCounter > 1)
                {
                    Reproduction(0);
                }

                if (algorithmStoppage || agentList.ActiveAgents.Length == 0)
                    break;

                Maintenance();
            }
        }
    }
}
