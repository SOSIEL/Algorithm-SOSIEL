// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

/// Description: Anticipatory learning is the first out of the learning
///   and decision making processes that is activated in the second and later
///   time periods. The aim of the process is to use the change in the states of
///   goal variables to update the anticipated influences of decision options,
///   assess the success of decision options, and gauge confidence in attaining
///   goals. The process of anticipatory learning consists of the following three
///   subprocesses: (a) update the anticipated influence(s) on goal(s) of the
///   decision option(s) that was/were implemented in the prior period;
///   (b) assess the success of this/these decision option(s) in contributing to
///   the attainment of goal(s); and (c) establish whether, by consequence, the
///   agent is confident or unconfident in attaining the goal(s). The results of
///   anticipatory learning are updated goal-specific anticipated influences and
///   confidence states.

using System;
using System.Collections.Generic;
using System.Linq;

using NLog;

using SOSIEL.Entities;
using SOSIEL.Enums;
using SOSIEL.Helpers;

namespace SOSIEL.Processes
{
    /// <summary>
    /// Anticipatory learning process implementation.
    /// </summary>
    public class AnticipatoryLearning : VolatileProcess
    {
        private static Logger _logger = LogHelper.GetLogger();

        /// <summary>
        /// Executes anticipatory learning for specific agent and returns sorted by priority goals array
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="iteration"></param>
        /// <returns></returns>
        public void Execute(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> iteration)
        {
            if (_logger.IsDebugEnabled) 
                _logger.Debug($"AnticipatoryLearning.Execute: agent={agent.Id}");

            var currentIterationAgentState = iteration.Value[agent];
            var previousIterationAgentState = iteration.Previous.Value[agent];

            foreach (var goal in agent.AssignedGoals)
            {
                var goalState = currentIterationAgentState.GoalStates[goal];
                goalState.Value = agent[goal.ReferenceVariable];
                var previousGoalState = previousIterationAgentState.GoalStates[goal];
                goalState.PriorValue = previousGoalState.Value;
                goalState.TwicePriorValue = previousGoalState.PriorValue;
                if (goal.ChangeFocalValueOnPrevious)
                {
                    double reductionPercent = 1;
                    if (goal.ReductionPercent > 0d)
                        reductionPercent = goal.ReductionPercent;
                    goalState.FocalValue = reductionPercent * goalState.PriorValue;
                }
                if (!string.IsNullOrEmpty(goal.FocalValueReferenceVariable))
                    goalState.FocalValue = agent[goal.FocalValueReferenceVariable];
                goalState.DiffCurrentAndFocal = goalState.Value - goalState.FocalValue;
                goalState.DiffPriorAndFocal = goalState.PriorValue - goalState.FocalValue;
                goalState.DiffCurrentAndPrior = goalState.Value - goalState.PriorValue;
                goalState.DiffPriorAndTwicePrior = goalState.PriorValue - previousGoalState.PriorValue;

                var anticipatedInfluence = goal.IsCumulative
                    ? goalState.Value - goalState.PriorValue
                    : goalState.Value;

                goalState.AnticipatedInfluenceValue = anticipatedInfluence;

                //finds activated decision option for each site
                var activatedInPriorIteration =
                    previousIterationAgentState.DecisionOptionHistories.SelectMany(rh => rh.Value.Activated);

                //update anticipated influences of found decision option
                activatedInPriorIteration.ForEach(r =>
                {
                    agent.AnticipationInfluence[r][goal] = anticipatedInfluence;
                });

                SpecificLogic(goalState);
            }

            //return SortByImportance(agent, currentIterationAgentState.GoalsState).ToArray();
        }
        #region Specific logic for tendencies
        protected override object EqualToOrAboveFocalValue(GoalState goalState, object customData)
        {
            if (goalState.Value >= goalState.FocalValue
                 || goalState.Value < goalState.FocalValue
                     && goalState.Value > goalState.PriorValue
                     && goalState.DiffCurrentAndPrior >= goalState.DiffPriorAndTwicePrior)
            {
                goalState.AnticipatedDirection = AnticipatedDirection.Stay;
                goalState.Confidence = true;
            }
            else
            {
                goalState.AnticipatedDirection = AnticipatedDirection.Up;
                goalState.Confidence = false;
            }
            return null;
        }

        protected override object Maximize(GoalState goalState, object customData)
        {
            if (goalState.Value == goalState.FocalValue
               || goalState.Value > goalState.PriorValue
                    && goalState.DiffCurrentAndPrior >= goalState.DiffPriorAndTwicePrior)
            {
                goalState.AnticipatedDirection = AnticipatedDirection.Stay;
                goalState.Confidence = true;
            }
            else
            {
                goalState.AnticipatedDirection = AnticipatedDirection.Up;
                goalState.Confidence = false;
            }
            return null;
        }

        protected override object Minimize(GoalState goalState, object customData)
        {
            if (goalState.Value == goalState.FocalValue
                || goalState.Value < goalState.PriorValue
                    && goalState.PriorValue - goalState.Value
                        >= goalState.TwicePriorValue - goalState.PriorValue)
            {
                goalState.AnticipatedDirection = AnticipatedDirection.Stay;
                goalState.Confidence = true;
            }
            else
            {
                goalState.AnticipatedDirection = AnticipatedDirection.Down;
                goalState.Confidence = false;
            }
            return null;
        }

        protected override object MaintainAtValue(GoalState goalState, object customData)
        {
            if (goalState.Value == goalState.FocalValue
                || Math.Abs(goalState.Value - goalState.FocalValue)
                    < Math.Abs(goalState.PriorValue - goalState.PriorFocalValue))
            {
                goalState.AnticipatedDirection = AnticipatedDirection.Stay;
                goalState.Confidence = true;
            }
            else
            {
                goalState.AnticipatedDirection = goalState.DiffCurrentAndFocal > 0
                    ? AnticipatedDirection.Down
                    : AnticipatedDirection.Up;
                goalState.Confidence = false;
            }
            return null;
        }

        #endregion
    }
}
