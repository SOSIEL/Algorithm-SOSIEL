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

        Goal _processedGoal;
        GoalState _goalState;


        Dictionary<DecisionOption, Dictionary<Goal, double>> _anticipatedInfluence;

        DecisionOption[] _matchedDecisionOptions;


        DecisionOption _priorPeriodActivatedDecisionOption;
        DecisionOption _decisionOptionForActivating;

        #region Specific logic for tendencies
        protected override void EqualToOrAboveFocalValue()
        {
            if(_goalState.Value >= _goalState.FocalValue) return;
            var selectedDecisionOptions = _matchedDecisionOptions;
            if (_matchedDecisionOptions.Length > 1)
            {
                selectedDecisionOptions = _matchedDecisionOptions.GroupBy(
                    r => _anticipatedInfluence[r][_processedGoal] - (_goalState.FocalValue - _goalState.Value))
                    .OrderBy(hg => hg.Key).First().ToArray();
            }
            _decisionOptionForActivating = selectedDecisionOptions.RandomizeOne();
        }

        protected override void Maximize()
        {
            if (_matchedDecisionOptions.Length > 0)
            {
                var selectedDecisionOptions = _matchedDecisionOptions.GroupBy(
                    r => _anticipatedInfluence[r][_processedGoal])
                    .OrderByDescending(hg => hg.Key).First().ToArray();

                _decisionOptionForActivating = selectedDecisionOptions.RandomizeOne();
            }
        }

        protected override void Minimize()
        {
            if (_matchedDecisionOptions.Length > 0)
            {
                var selectedDecisionOptions = _matchedDecisionOptions
                    .GroupBy(r => _anticipatedInfluence[r][_processedGoal]).OrderBy(hg => hg.Key).First().ToArray();

                _decisionOptionForActivating = selectedDecisionOptions.RandomizeOne();
            }
        }

        protected override void MaintainAtValue()
        {
            if(_goalState.Value == _goalState.FocalValue) return;
            var selectedDecisionOptions = _matchedDecisionOptions;
            if (_matchedDecisionOptions.Length > 1)
            {
                selectedDecisionOptions = _matchedDecisionOptions.GroupBy(
                    r => _anticipatedInfluence[r][_processedGoal] - Math.Abs(_goalState.FocalValue - _goalState.Value))
                  .OrderBy(hg => hg.Key).First().ToArray();
            }
            _decisionOptionForActivating = selectedDecisionOptions.RandomizeOne();
        }
        #endregion

        /// <summary>
        /// Shares collective action among same household agents
        /// </summary>
        /// <param name="currentAgent"></param>
        /// <param name="decisionOption"></param>
        /// <param name="agentStates"></param>
        List<IAgent> SignalingInterest(IAgent currentAgent, DecisionOption decisionOption,
            Dictionary<IAgent, AgentState> agentStates)
        {
            if (_logger.IsDebugEnabled)
                _logger.Debug($"Satisficing.SignalingInterest: {currentAgent.Id}");
            var scope = decisionOption.Scope;
            var agents = new List<IAgent>();
            foreach (IAgent neighbour in currentAgent.ConnectedAgents
                .Where(connected => scope == null || (connected.ContainsVariable(scope)
                                                      && currentAgent.ContainsVariable(scope)
                                                      && connected[scope] == currentAgent[scope])))
            {
                if (neighbour.AssignedDecisionOptions.Contains(decisionOption) == false)
                {
                    neighbour.AssignNewDecisionOption(
                        decisionOption, currentAgent.AnticipationInfluence[decisionOption]);
                    agents.Add(neighbour);
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
        /// <param name="processedDecisionOptions"></param>
        /// <param name="dataSet"></param>
        public void ExecutePartI(
            int recursionLevel,
            IAgent agent,
            LinkedListNode<Dictionary<IAgent, AgentState>> currentIterationNode,
            Goal[] rankedGoals,
            DecisionOption[] processedDecisionOptions,
            IDataSet dataSet
        )
        {
            if (_logger.IsDebugEnabled)
                _logger.Debug($"Satisficing.ExecutePartI: recursionLevel={recursionLevel} agent={agent.Id}");

            _decisionOptionForActivating = null;

            var currentAgentState = currentIterationNode.Value[agent];
            var priorPeriodAgentState = currentIterationNode.Previous?.Value[agent];

            //adds new decisionOption history for specific data set if it doesn't exist
            DecisionOptionHistory decisionOptionHistory;
            if (!currentAgentState.DecisionOptionHistories.TryGetValue(dataSet, out decisionOptionHistory))
            {
                decisionOptionHistory = new DecisionOptionHistory();
                currentAgentState.DecisionOptionHistories.Add(dataSet, decisionOptionHistory);
            }

            _processedGoal = rankedGoals.First(
                g => processedDecisionOptions.First().Layer.Set.AssociatedWith.Contains(g));
            _goalState = currentAgentState.GoalStates[_processedGoal];
            _matchedDecisionOptions = processedDecisionOptions.Except(decisionOptionHistory.Blocked)
                .Where(h => h.IsMatch(agent)).ToArray();

            switch (_matchedDecisionOptions.Length)
            {
                case 0: return;
                case 1: _decisionOptionForActivating = _matchedDecisionOptions[0]; break;
                default:
                {
                    if (priorPeriodAgentState != null)
                    {
                        _priorPeriodActivatedDecisionOption = priorPeriodAgentState.DecisionOptionHistories[dataSet]
                            .Activated.FirstOrDefault(r => r.Layer == processedDecisionOptions.First().Layer);
                    }
                    //set anticipated influence before execute specific logic
                    _anticipatedInfluence = agent.AnticipationInfluence;
                    SpecificLogic(_processedGoal.Tendency);
                    break;
                }
            }

            if (processedDecisionOptions.First().Layer.Set.Layers.Count > 1)
                _decisionOptionForActivating.Apply(agent);

            if (_decisionOptionForActivating != null)
                decisionOptionHistory.Activated.Add(_decisionOptionForActivating);

            decisionOptionHistory.Matched.AddRange(_matchedDecisionOptions);

            if (_decisionOptionForActivating != null && _decisionOptionForActivating.IsCollectiveAction)
            {
                var agents = SignalingInterest(agent, _decisionOptionForActivating, currentIterationNode.Value);
                foreach (var agent1 in agents)
                {
                    var agentHistory = currentIterationNode.Value[agent1].DecisionOptionHistories[dataSet];
                    var layer = _decisionOptionForActivating.Layer;
                    if (agentHistory.Activated.Any(h => h.Layer == layer))
                    {
                        //clean previous choice
                        agentHistory.Activated.RemoveAll(h => h.Layer == layer);
                        agentHistory.Matched.RemoveAll(h => h.Layer == layer);
                        var decisionOptions = agent1.AssignedDecisionOptions.Where(h => h.Layer == layer).ToArray();
                        ExecutePartI(
                            recursionLevel + 1, agent1, currentIterationNode, rankedGoals, decisionOptions, dataSet);
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
        /// <param name="processedDecisionOptions"></param>
        /// <param name="dataSet"></param>
        public void ExecutePartII(
            int recursionLevel,
            IAgent agent,
            LinkedListNode<Dictionary<IAgent, AgentState>> currentIterationNode,
            Goal[] rankedGoals,
            DecisionOption[] processedDecisionOptions,
            IDataSet dataSet
         )
        {
            if (_logger.IsDebugEnabled)
                _logger.Debug($"Satisficing.ExecutePartII: recursionLevel={recursionLevel} agent={agent.Id}");
            var agentState = currentIterationNode.Value[agent];
            var decisionOptionHistory = agentState.DecisionOptionHistories[dataSet];
            var layer = processedDecisionOptions.First().Layer;
            var selectedDecisionOption = decisionOptionHistory.Activated
                .SingleOrDefault(r => r.Layer == layer);
            if (selectedDecisionOption != null && selectedDecisionOption.IsCollectiveAction)
            {
                var scope = selectedDecisionOption.Scope;
                // counting agents which selected this decision option
                int numberOfInvolvedAgents = agent.ConnectedAgents.Where(
                    connected => scope == null || agent[scope] == connected[scope])
                    .Count(a => currentIterationNode.Value[a].DecisionOptionHistories[dataSet]
                    .Activated.Any(decisionOption => decisionOption == selectedDecisionOption));
                int requiredParticipants = selectedDecisionOption.RequiredParticipants - 1;
                // add decision option to blocked
                if (numberOfInvolvedAgents < requiredParticipants)
                {
                    decisionOptionHistory.Blocked.Add(selectedDecisionOption);
                    decisionOptionHistory.Activated.Remove(selectedDecisionOption);
                    ExecutePartI(
                        recursionLevel + 1, agent, currentIterationNode, rankedGoals,
                        processedDecisionOptions, dataSet
                    );
                    ExecutePartII(
                        recursionLevel + 1, agent, currentIterationNode, rankedGoals,
                        processedDecisionOptions, dataSet
                    );
                }
            }
        }
    }
}
