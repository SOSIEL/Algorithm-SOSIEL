// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

/// Description: Goal selecting is the first cognitive process activated during
///   the first time period and subsequently the first activated decision-making
///   process in the second and later time periods. The aim of goal selecting is to
///   generate a list of goals from which goals of focus can be selected during
///   decision-making and in which the goals are ordered by their importance levels
///   (which, during the second and later time periods are updated during anticipatory
///   learning). The reason a list of goals is generated (as opposed to a single
///   goal) is because not all mental (sub)models are associated with all goals.
///   The process of goal selecting consists of the following two subprocesses:
///   (a) generate the goal importance distribution, which constructs a distribution
///   of goals that reflects their importance levels; and (b) generate the goals
///   of focus list, which applies a uniform distribution to randomly select a list
///   of goals from the goal importance distribution. The result of the goal
///   selecting process is a list of goals, the goals of focus list, approximately
///   ordered by their level of importance. The ordering is only approximate because
///   the use of a uniform distribution to select goals implies that, at any point,
///   chance may lead to the selection of a less important goal, thereby introducing
///   a degree of uncertainty. The process of signaling interest in a collective
///   action follows satisficing when the selected decision during satisficing is
///   a collective action and interest in it has not yet been signaled to members
///   of associated social networks. The result of signaling interest in a collective
///   action is an updated list of agents committed to the collective action. After
///   all agents interested in collective action had a chance to express their
///   interest, the process of satisficing is reactivated. If a sufficient number
///   of agents have signaled interest in a collective action, then during
///   satisficing the collective action becomes their selected decision. If, however,
///   the number of agents signaling interest is not sufficient, then the collective
///   action is deactivated as a potential decision option during the current period
///   and the agents reengage in satisficing.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using NLog;

using SOSIEL.Entities;
using SOSIEL.Helpers;

namespace SOSIEL.Processes
{
    /// <summary>
    /// Action selection process implementation.
    /// </summary>
    public class Satisficing : VolatileProcess
    {
        private static Logger _logger = LogHelper.GetLogger();

        private class SpecificLogicCustomData
        {
            public DecisionOption[] MatchedDecisionOptions { get; set; }
            public Dictionary<DecisionOption, Dictionary<Goal, double>> AnticipatedInfluence { get; set; }
        }

        /// <summary>
        /// Shares collective action among same household agents
        /// </summary>
        /// <param name="currentAgent"></param>
        /// <param name="decisionOption"></param>
        List<IAgent> SignalingInterest(IAgent currentAgent, DecisionOption decisionOption)
        {
            if (_logger.IsDebugEnabled)
                _logger.Debug($"Satisficing.SignalingInterest: {currentAgent.Id}");
            var scope = decisionOption.Scope;
            var agents = new List<IAgent>();
            var hasScope = currentAgent.ContainsVariable(scope);
            foreach (IAgent neighbor in currentAgent.ConnectedAgents.Where(
                connectedAgent => scope == null || (hasScope && connectedAgent.ContainsVariable(scope)
                                                      && connectedAgent[scope] == currentAgent[scope])))
            {
                if (!neighbor.AssignedDecisionOptions.Contains(decisionOption))
                {
                    var influence = currentAgent.AnticipationInfluence[decisionOption];
                    neighbor.AssignNewDecisionOption(decisionOption, influence);
                    agents.Add(neighbor);
                }
            }
            return agents;
        }

        /// <summary>
        /// Executes first part of action selection for specific agent and data set
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="currentIterationNode"></param>
        /// <param name="rankedGoals"></param>
        /// <param name="decisionOptions"></param>
        /// <param name="dataSet"></param>
        public void ExecutePartI(
            int recursionLevel,
            IAgent agent,
            LinkedListNode<Dictionary<IAgent, AgentState>> currentIterationNode,
            Goal[] rankedGoals,
            DecisionOption[] decisionOptions,
            IDataSet dataSet
        )
        {
            if (_logger.IsDebugEnabled)
                _logger.Debug($"Satisficing.ExecutePartI: recursionLevel={recursionLevel} agent={agent.Id}");

            var currentAgentState = currentIterationNode.Value[agent];
            var priorPeriodAgentState = currentIterationNode.Previous?.Value[agent];

            DecisionOptionHistory decisionOptionHistory;
            if (!currentAgentState.DecisionOptionHistories.TryGetValue(dataSet, out decisionOptionHistory))
            {
                decisionOptionHistory = new DecisionOptionHistory();
                currentAgentState.DecisionOptionHistories.Add(dataSet, decisionOptionHistory);
            }

            var firstDecisionOptionGoals = decisionOptions.First().ParentLayer.ParentMentalModel.AssociatedGoals;
            //Debugger.Launch();
            var goal = rankedGoals.First(g => firstDecisionOptionGoals.Contains(g));
            var goalState = currentAgentState.GoalStates[goal];
            var matchedDecisionOptions = decisionOptions.Except(decisionOptionHistory.Blocked)
                .Where(h => h.IsMatch(agent)).ToArray();

            DecisionOption decisionOptionToActivate = null;
            switch (matchedDecisionOptions.Length)
            {
                case 0: return;
                case 1: decisionOptionToActivate = matchedDecisionOptions[0]; break;
                default:
                {
                    //set anticipated influence before execute specific logic
                    decisionOptionToActivate = (DecisionOption)SpecificLogic(
                        goalState,
                        new SpecificLogicCustomData
                        {
                            MatchedDecisionOptions = matchedDecisionOptions,
                            AnticipatedInfluence = agent.AnticipationInfluence
                        }
                        );
                    break;
                }
            }

            if (decisionOptionToActivate != null)
            {
                if (decisionOptions.First().ParentLayer.ParentMentalModel.Layers.Count > 1)
                    decisionOptionToActivate.Apply(agent);
                decisionOptionHistory.Activated.Add(decisionOptionToActivate);
            }

            decisionOptionHistory.Matched.AddRange(matchedDecisionOptions);

            if (decisionOptionToActivate != null && decisionOptionToActivate.IsCollectiveAction)
            {
                var signaledAgents = SignalingInterest(agent, decisionOptionToActivate);
                foreach (var signaledAgent in signaledAgents)
                {
                    var agentHistory = currentIterationNode.Value[signaledAgent].DecisionOptionHistories[dataSet];
                    var layer = decisionOptionToActivate.ParentLayer;
                    if (agentHistory.Activated.Any(h => h.ParentLayer == layer))
                    {
                        // clear previous choices
                        agentHistory.Activated.RemoveAll(h => h.ParentLayer == layer);
                        agentHistory.Matched.RemoveAll(h => h.ParentLayer == layer);
                        var decisionOptions1 = signaledAgent.AssignedDecisionOptions
                            .Where(h => h.ParentLayer == layer).ToArray();
                        ExecutePartI(recursionLevel + 1, signaledAgent, currentIterationNode,
                            rankedGoals, decisionOptions1, dataSet);
                    }
                }
            }
        }

        /// <summary>
        /// Executes second part of action selection for specific data set
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="currentIterationNode"></param>
        /// <param name="rankedGoals"></param>
        /// <param name="decisionOptions"></param>
        /// <param name="dataSet"></param>
        public void ExecutePartII(
            int recursionLevel,
            IAgent agent,
            LinkedListNode<Dictionary<IAgent, AgentState>> currentIterationNode,
            Goal[] rankedGoals,
            DecisionOption[] decisionOptions,
            IDataSet dataSet
         )
        {
            if (_logger.IsDebugEnabled)
                _logger.Debug($"Satisficing.ExecutePartII: recursionLevel={recursionLevel} agent={agent.Id}");
            var agentState = currentIterationNode.Value[agent];
            var decisionOptionHistory = agentState.DecisionOptionHistories[dataSet];
            var layer = decisionOptions.First().ParentLayer;
            var selectedDecisionOption = decisionOptionHistory.Activated.SingleOrDefault(r => r.ParentLayer == layer);
            if (selectedDecisionOption != null && selectedDecisionOption.IsCollectiveAction)
            {
                var scope = selectedDecisionOption.Scope;
                // counting agents which selected this decision option
                int numberOfInvolvedAgents = agent.ConnectedAgents.Where(
                    connected => scope == null || agent[scope] == connected[scope])
                    .Count(a => currentIterationNode.Value[a].DecisionOptionHistories[dataSet]
                    .Activated.Any(decisionOption => decisionOption == selectedDecisionOption));
                int requiredNumberOfParticipants = selectedDecisionOption.RequiredParticipants - 1;
                // add decision option to blocked
                if (numberOfInvolvedAgents < requiredNumberOfParticipants)
                {
                    decisionOptionHistory.Blocked.Add(selectedDecisionOption);
                    decisionOptionHistory.Activated.Remove(selectedDecisionOption);
                    ExecutePartI(
                        recursionLevel + 1, agent, currentIterationNode, rankedGoals,
                        decisionOptions, dataSet
                    );
                    ExecutePartII(
                        recursionLevel + 1, agent, currentIterationNode, rankedGoals,
                        decisionOptions, dataSet
                    );
                }
            }
        }

        #region Specific logic for tendencies
        protected override object EqualToOrAboveFocalValue(GoalState goalState, object customData)
        {
            if (goalState.Value >= goalState.FocalValue) return null;
            var data = (SpecificLogicCustomData)customData;
            var selectedDecisionOptions = data.MatchedDecisionOptions;
            if (data.MatchedDecisionOptions.Length > 1)
            {
                selectedDecisionOptions = data.MatchedDecisionOptions.GroupBy(
                    r => data.AnticipatedInfluence[r][goalState.Goal] - (goalState.FocalValue - goalState.Value))
                    .OrderBy(hg => hg.Key).First().ToArray();
            }
            return selectedDecisionOptions.ChooseRandomElement();
        }

        protected override object Maximize(GoalState goalState, object customData)
        {
            var data = (SpecificLogicCustomData)customData;
            if (data.MatchedDecisionOptions.Length == 0) return null;
            var selectedDecisionOptions = data.MatchedDecisionOptions.GroupBy(
                r => data.AnticipatedInfluence[r][goalState.Goal])
                .OrderByDescending(hg => hg.Key).First().ToArray();
            return selectedDecisionOptions.ChooseRandomElement();
        }

        protected override object Minimize(GoalState goalState, object customData)
        {
            var data = (SpecificLogicCustomData)customData;
            if (data.MatchedDecisionOptions.Length == 0) return null;
            var selectedDecisionOptions = data.MatchedDecisionOptions
                .GroupBy(r => data.AnticipatedInfluence[r][goalState.Goal]).OrderBy(hg => hg.Key).First().ToArray();
            return selectedDecisionOptions.ChooseRandomElement();
        }

        protected override object MaintainAtValue(GoalState goalState, object customData)
        {
            if (goalState.Value == goalState.FocalValue) return null;
            var data = (SpecificLogicCustomData)customData;
            var selectedDecisionOptions = data.MatchedDecisionOptions;
            if (data.MatchedDecisionOptions.Length > 1)
            {
                selectedDecisionOptions = data.MatchedDecisionOptions.GroupBy(
                    r => data.AnticipatedInfluence[r][goalState.Goal] - Math.Abs(goalState.FocalValue - goalState.Value))
                  .OrderBy(hg => hg.Key).First().ToArray();
            }
            return selectedDecisionOptions.ChooseRandomElement();
        }
        #endregion
    }
}
