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

        private class SpecificLogicCustomData
        {
            public DecisionOption[] MatchedDecisionOptions { get; set; }
            public Dictionary<DecisionOption, Dictionary<Goal, double>> AnticipatedInfluence { get; set; }
            public DecisionOption ActivatedDecisionOption { get; set;  }
        }

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

            var matchedDecisionOptions = prevIterationAgentState.DecisionOptionHistories[site]
                .Matched.Where(h => h.ParentLayer == layer).ToArray();
            if (matchedDecisionOptions.Length < 2) return false;

            var goalState = iterationNode.Value[agent].GoalStates[goal];
            goalState.Confidence = false;
            var history = prevIterationAgentState.DecisionOptionHistories[site];
            var activatedDecisionOption = history.Activated.FirstOrDefault(r => r.ParentLayer == layer);
            
            // First, copy old influences
            var anticipatedInfluences = new Dictionary<DecisionOption, Dictionary<Goal, double>>();
            foreach (var kvp in prevIterationAgentState.AnticipatedInfluences)
                anticipatedInfluences.Add(kvp.Key, new Dictionary<Goal, double>(kvp.Value));

            // Update with new influences where applicable
            foreach (var kvp in agent.AnticipationInfluence)
            {
                Dictionary<Goal, double> influences;
                if (anticipatedInfluences.TryGetValue(kvp.Key, out influences))
                {
                    foreach (var kvp2 in kvp.Value)
                        influences[kvp2.Key] = kvp2.Value;
                }
            }

            if (_logger.IsDebugEnabled)
            {
                _logger.Debug($"CounterfactualThinking.Execute: agent={agent.Id} goal={goal}"
                    + $" layer={layer.LayerId}\n"
                    + $"  Matched DOs: {string.Join(", ", matchedDecisionOptions.Select(x => x.Name).OrderBy(x => x))}");
                _logger.Debug("  AnticipatedInfluences: "
                    + $"{string.Join(", ", anticipatedInfluences.Select(x => x.Key.Name).OrderBy(x => x))}\n");
            }
            SpecificLogic(
                goalState,
                new SpecificLogicCustomData
                {
                    MatchedDecisionOptions = matchedDecisionOptions,
                    AnticipatedInfluence = anticipatedInfluences,
                    ActivatedDecisionOption = activatedDecisionOption
                }
                );
            return goalState.Confidence;
        }

        #region Specific logic for tendencies

        protected override object EqualToOrAboveFocalValue(GoalState goalState, object customData)
        {
            if (goalState.PriorValue < goalState.PriorFocalValue)
            {
                var dv = goalState.PriorFocalValue - goalState.PriorValue;
                var data = (SpecificLogicCustomData)customData;
                var decisionOptions = (data.MatchedDecisionOptions.Length > 1)
                    ? data.MatchedDecisionOptions
                        .GroupBy(r => data.AnticipatedInfluence[r][goalState.Goal] - dv)
                        .OrderBy(hg => hg.Key)
                        .First()
                        .ToArray()
                    : data.MatchedDecisionOptions;
                goalState.Confidence = decisionOptions.Length > 0
                    && decisionOptions.Any(r => r != data.ActivatedDecisionOption);
            }
            return null;
        }

        protected override object Maximize(GoalState goalState, object customData)
        {
            var data = (SpecificLogicCustomData)customData;
            if (data.MatchedDecisionOptions.Length > 0)
            {
                var decisionOptions = data.MatchedDecisionOptions
                    .GroupBy(r => data.AnticipatedInfluence[r][goalState.Goal])
                    .OrderByDescending(hg => hg.Key)
                    .First()
                    .ToArray();
                goalState.Confidence = decisionOptions.Length > 0 
                    && decisionOptions.Any(r => r != data.ActivatedDecisionOption);
            }
            return null;
        }

        protected override object Minimize(GoalState goalState, object customData)
        {
            var data = (SpecificLogicCustomData)customData;
            if (data.MatchedDecisionOptions.Length > 0)
            {
                var decisionOptions = data.MatchedDecisionOptions
                    .GroupBy(r => data.AnticipatedInfluence[r][goalState.Goal])
                    .OrderBy(hg => hg.Key)
                    .First()
                    .ToArray();
                goalState.Confidence = decisionOptions.Length > 0
                    && decisionOptions.Any(r => r != data.ActivatedDecisionOption);
            }
            return null;
        }

        protected override object MaintainAtValue(GoalState goalState, object customData)
        {
            if (goalState.PriorValue != goalState.PriorFocalValue)
            {
                var data = (SpecificLogicCustomData)customData;
                var dv = Math.Abs(goalState.PriorFocalValue - goalState.PriorValue);
                var decisionOptions = (data.MatchedDecisionOptions.Length > 1)
                    ? data.MatchedDecisionOptions
                        .GroupBy(r => data.AnticipatedInfluence[r][goalState.Goal] - dv)
                        .OrderBy(hg => hg.Key)
                        .First()
                        .ToArray()
                    : data.MatchedDecisionOptions;
                goalState.Confidence = decisionOptions.Length > 0
                    && decisionOptions.Any(r => r != data.ActivatedDecisionOption);
            }
            return null;
        }

        #endregion
    }
}
