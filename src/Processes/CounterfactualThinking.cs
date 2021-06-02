// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

/// Description:
///   Counterfactual thinking follows goal prioritizing only in the
///   case that a mental (sub)model is modifiable, there is a lack of confidence
///   in relation to a goal, and the number of decision options matching conditions
///   in the prior period was equal to or greater than two. A loss of confidence,
///   which may occur during the process of anticipatory learning, triggers
///   counterfactual thinking as an effort to explain the discrepancy between the
///   anticipated and actual results of a decision. The aim of counterfactual
///   thinking is to check whether or not the agent would have behaved differently
///   (i.e., if an available alternate decision had been selected) had it known
///   in the prior period (which is represented by a prior set of conditions) what
///   it knows in the current (which is represented by updated anticipations). If
///   an alternative satisfactory decision is identified, then confidence is
///   regained and the agent moves on to the process of social learning. If, however,
///   an alternative decision is not identified, then the agent remains unconfident
///   and continues with individual learning by engaging in innovating, before moving
///   on to social learning. The process of counterfactual thinking consists of the
///   following two subprocesses: (a) search for a better decision option and
///   (b) assess the success of the search. The result of counterfactual thinking
///   is knowledge of whether a potentially better decision option is present in
///   the corresponding mental (sub)model and whether there is a potential change
///   to the state of uncertainty.

using System;
using System.Collections.Generic;
using System.Linq;

using NLog;

using SOSIEL.Entities;
using SOSIEL.Helpers;

namespace SOSIEL.Processes
{
    /// <summary>
    /// Counterfactual thinking process implementation.
    /// </summary>
    public class CounterfactualThinking : VolatileProcess
    {
        private static Logger _logger = LogHelper.GetLogger();

        private Goal _selectedGoal;
        private GoalState _selectedGoalState;
        private Dictionary<DecisionOption, Dictionary<Goal, double>> _anticipatedInfluences;
        private DecisionOption[] _matchedDecisionOptions;
        private DecisionOption _activatedDecisionOption;

        /// <summary>
        /// Executes counterfactual thinking about most important agent goal for specific site
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="iterationNode"></param>
        /// <param name="goal"></param>
        /// <param name="matched"></param>
        /// <param name="layer"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public bool Execute(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> iterationNode,
            Goal goal, DecisionOptionLayer layer, IDataSet site)
        {
            var prevIterationAgentState = iterationNode.Previous.Value[agent];

            _matchedDecisionOptions = prevIterationAgentState.DecisionOptionHistories[site]
                .Matched.Where(h => h.Layer == layer).ToArray();
            if (_matchedDecisionOptions.Length < 2) return false;

            _selectedGoal = goal;
            _selectedGoalState = iterationNode.Value[agent].GoalStates[_selectedGoal];
            _selectedGoalState.Confidence = false;
            var history = prevIterationAgentState.DecisionOptionHistories[site];
            _activatedDecisionOption = history.Activated.FirstOrDefault(r => r.Layer == layer);
            
            // First, copy old influences
            _anticipatedInfluences = new Dictionary<DecisionOption, Dictionary<Goal, double>>();
            foreach (var kvp in prevIterationAgentState.AnticipatedInfluences)
                _anticipatedInfluences.Add(kvp.Key, new Dictionary<Goal, double>(kvp.Value));

            // Update with new influences where applicable
            foreach (var kvp in agent.AnticipationInfluence)
            {
                Dictionary<Goal, double> influences;
                if (_anticipatedInfluences.TryGetValue(kvp.Key, out influences))
                {
                    foreach (var kvp2 in kvp.Value)
                        influences[kvp2.Key] = kvp2.Value;
                }
            }

            if (_logger.IsDebugEnabled)
            {
                _logger.Debug($"CounterfactualThinking.Execute: agent={agent.Id} goal={goal}"
                    + $" layer={layer.PositionNumber}\n"
                    + $"  Matched DOs: {string.Join(", ", _matchedDecisionOptions.Select(x => x.Id).OrderBy(x => x))}");
                _logger.Debug("  AnticipatedInfluences: "
                    + $"{string.Join(", ", _anticipatedInfluences.Select(x => x.Key.Id).OrderBy(x => x))}\n");
            }
            SpecificLogic(_selectedGoal.Tendency);
            return _selectedGoalState.Confidence;
        }

        #region Specific logic for tendencies

        protected override void EqualToOrAboveFocalValue()
        {
            if (_selectedGoalState.PriorValue >= _selectedGoalState.PriorFocalValue) return;
            var dv = _selectedGoalState.PriorFocalValue - _selectedGoalState.PriorValue;
            var decisionOptions = (_matchedDecisionOptions.Length > 1)
                ? _matchedDecisionOptions
                    .GroupBy(r => GetAnticipatedInfluence(r)[_selectedGoal] - dv)
                    .OrderBy(hg => hg.Key)
                    .First()
                    .ToArray()
                : _matchedDecisionOptions;
            _selectedGoalState.Confidence = decisionOptions.Length > 0 
                && decisionOptions.Any(r => r != _activatedDecisionOption);
        }

        private Dictionary<Goal, double> GetAnticipatedInfluence(DecisionOption decisionOption)
        {
            return _anticipatedInfluences[decisionOption];
        }

        protected override void Maximize()
        {
            if (_matchedDecisionOptions.Length > 0)
            {
                var decisionOptions = _matchedDecisionOptions
                    .GroupBy(r => GetAnticipatedInfluence(r)[_selectedGoal])
                    .OrderByDescending(hg => hg.Key)
                    .First()
                    .ToArray();
                _selectedGoalState.Confidence = decisionOptions.Length > 0 
                    && decisionOptions.Any(r => r != _activatedDecisionOption);
            }
        }

        protected override void Minimize()
        {
            if (_matchedDecisionOptions.Length > 0)
            {
                var decisionOptions = _matchedDecisionOptions.GroupBy(r => GetAnticipatedInfluence(r)[_selectedGoal])
                    .OrderBy(hg => hg.Key)
                    .First()
                    .ToArray();
                _selectedGoalState.Confidence = decisionOptions.Length > 0
                    && decisionOptions.Any(r => r != _activatedDecisionOption);
            }
        }

        protected override void MaintainAtValue()
        {
            if (_selectedGoalState.PriorValue == _selectedGoalState.PriorFocalValue) return;
            var dv = Math.Abs(_selectedGoalState.PriorFocalValue - _selectedGoalState.PriorValue);
            var decisionOptions = (_matchedDecisionOptions.Length > 1)
                ? _matchedDecisionOptions
                    .GroupBy(r => GetAnticipatedInfluence(r)[_selectedGoal] - dv)
                    .OrderBy(hg => hg.Key)
                    .First()
                    .ToArray()
                : _matchedDecisionOptions;
            _selectedGoalState.Confidence = decisionOptions.Length > 0
                && decisionOptions.Any(r => r != _activatedDecisionOption);
        }

        #endregion
    }
}
