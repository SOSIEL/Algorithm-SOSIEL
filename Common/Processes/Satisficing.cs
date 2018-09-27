using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Processes
{
    using Entities;
    using Helpers;
    using Enums;
    using Exceptions;


    /// <summary>
    /// Action selection process implementation.
    /// </summary>
    public class Satisficing : VolatileProcess
    {
        Goal processedGoal;
        GoalState goalState;


        Dictionary<DecisionOption, Dictionary<Goal, double>> anticipatedInfluence;

        DecisionOption[] matchedDecisionOptions;


        DecisionOption priorPeriodActivatedDecisionOption;
        DecisionOption decisionOptionForActivating;

        #region Specific logic for tendencies
        protected override void EqualToOrAboveFocalValue()
        {
            //We don't do anything. Do nothing decisionOption will be selected later.
        }

        protected override void Maximize()
        {
            if (matchedDecisionOptions.Length > 0)
            {
                DecisionOption[] selected = matchedDecisionOptions.GroupBy(r => anticipatedInfluence[r][processedGoal]).OrderByDescending(hg => hg.Key).First().ToArray();

                decisionOptionForActivating = selected.RandomizeOne();
            }
        }

        protected override void Minimize()
        {
            if (matchedDecisionOptions.Length > 0)
            {
                DecisionOption[] selected = matchedDecisionOptions.GroupBy(r => anticipatedInfluence[r][processedGoal]).OrderBy(hg => hg.Key).First().ToArray();

                decisionOptionForActivating = selected.RandomizeOne();
            }
        }
        #endregion

        /// <summary>
        /// Shares collective action among same household agents
        /// </summary>
        /// <param name="currentAgent"></param>
        /// <param name="decisionOption"></param>
        /// <param name="agentStates"></param>
        List<IAgent> SignalingInterest(IAgent currentAgent, DecisionOption decisionOption, Dictionary<IAgent, AgentState> agentStates)
        {
            var scope = decisionOption.Scope;

            var agents = new List<IAgent>();

            foreach (IAgent neighbour in currentAgent.ConnectedAgents
                .Where(connected => connected[scope] == currentAgent[scope] || scope == null))
            {
                if (neighbour.AssignedDecisionOptions.Contains(decisionOption) == false)
                {
                    neighbour.AssignNewDecisionOption(decisionOption, currentAgent.AnticipationInfluence[decisionOption]);
                    agents.Add(neighbour);
                }
            }

            return agents;
        }

        /// <summary>
        /// Executes first part of action selection for specific agent and site
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="lastIteration"></param>
        /// <param name="rankedGoals"></param>
        /// <param name="processedDecisionOptions"></param>
        /// <param name="site"></param>
        public void ExecutePartI(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> lastIteration, Dictionary<IAgent, Goal[]> rankedGoals, DecisionOption[] processedDecisionOptions, Site site)
        {
            decisionOptionForActivating = null;

            AgentState agentState = lastIteration.Value[agent];
            AgentState priorPeriod = lastIteration.Previous?.Value[agent];

            //adds new decisionOption history for specific site if it doesn't exist
            if (agentState.DecisionOptionsHistories.ContainsKey(site) == false)
                agentState.DecisionOptionsHistories.Add(site, new DecisionOptionsHistory());

            DecisionOptionsHistory history = agentState.DecisionOptionsHistories[site];

            processedGoal = rankedGoals[agent].First(g => processedDecisionOptions.First().Layer.Set.AssociatedWith.Contains(g));
            goalState = agentState.GoalsState[processedGoal];

            matchedDecisionOptions = processedDecisionOptions.Except(history.Blocked).Where(h => h.IsMatch(agent)).ToArray();

            if (matchedDecisionOptions.Length == 0)
            {
                return;
            }

            if (matchedDecisionOptions.Length > 1)
            {
                if (priorPeriod != null)
                    priorPeriodActivatedDecisionOption = priorPeriod.DecisionOptionsHistories[site].Activated.FirstOrDefault(r => r.Layer == processedDecisionOptions.First().Layer);

                //set anticipated influence before execute specific logic
                anticipatedInfluence = agent.AnticipationInfluence;

                SpecificLogic(processedGoal.Tendency);
            }
            else
                decisionOptionForActivating = matchedDecisionOptions[0];

            if (processedDecisionOptions.First().Layer.Set.Layers.Count > 1)
                decisionOptionForActivating.Apply(agent);

            if (decisionOptionForActivating != null)
            {
                history.Activated.Add(decisionOptionForActivating);
            }

            history.Matched.AddRange(matchedDecisionOptions);

            if (decisionOptionForActivating != null && decisionOptionForActivating.IsCollectiveAction)
            {
                var agents = SignalingInterest(agent, decisionOptionForActivating, lastIteration.Value);

                if (agents.Count > 0)
                {
                    foreach (var a in agents)
                    {
                        var agentHistory = lastIteration.Value[a].DecisionOptionsHistories[site];
                        var layer = decisionOptionForActivating.Layer;
                        if (agentHistory.Activated.Any(h => h.Layer == layer))
                        {
                            //clean previous choice
                            agentHistory.Activated.RemoveAll(h => h.Layer == layer);
                            agentHistory.Matched.RemoveAll(h => h.Layer == layer);

                            var decisionOpts = a.AssignedDecisionOptions.Where(h => h.Layer == layer).ToArray();

                            ExecutePartI(a, lastIteration, rankedGoals, decisionOpts, site);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Executes second part of action selection for specific site
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="lastIteration"></param>
        /// <param name="rankedGoals"></param>
        /// <param name="processedDecisionOptions"></param>
        /// <param name="site"></param>
        public void ExecutePartII(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> lastIteration, Dictionary<IAgent, Goal[]> rankedGoals, DecisionOption[] processedDecisionOptions, Site site)
        {
            AgentState agentState = lastIteration.Value[agent];

            DecisionOptionsHistory history = agentState.DecisionOptionsHistories[site];

            DecisionOptionLayer layer = processedDecisionOptions.First().Layer;


            DecisionOption selectedDecisionOptions = history.Activated.SingleOrDefault(r => r.Layer == layer);

            if (selectedDecisionOptions == null) return;

            if (selectedDecisionOptions.IsCollectiveAction)
            {
                var scope = selectedDecisionOptions.Scope;

                //counting agents which selected this decision option
                int numberOfInvolvedAgents = agent.ConnectedAgents.Where(connected => agent[scope] == connected[scope] || scope == null)
                    .Count(a => lastIteration.Value[a].DecisionOptionsHistories[site].Activated.Any(decisionOption => decisionOption == selectedDecisionOptions));

                int requiredParticipants = selectedDecisionOptions.RequiredParticipants - 1;

                //add decision option to blocked
                if (numberOfInvolvedAgents < requiredParticipants)
                {
                    history.Blocked.Add(selectedDecisionOptions);

                    history.Activated.Remove(selectedDecisionOptions);

                    ExecutePartI(agent, lastIteration, rankedGoals, processedDecisionOptions, site);

                    ExecutePartII(agent, lastIteration, rankedGoals, processedDecisionOptions, site);
                }
            }
        }
    }
}

