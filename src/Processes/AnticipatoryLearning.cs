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

        Goal _currentGoal;
        GoalState _currentGoalState;

        #region Specific logic for tendencies
        protected override void EqualToOrAboveFocalValue()
        {
            if (_currentGoalState.Value >= _currentGoalState.FocalValue
                 || _currentGoalState.Value < _currentGoalState.FocalValue
                     && _currentGoalState.Value > _currentGoalState.PriorValue
                     && _currentGoalState.DiffCurrentAndPrior >= _currentGoalState.DiffPriorAndTwicePrior)
            {
                _currentGoalState.AnticipatedDirection = AnticipatedDirection.Stay;
                _currentGoalState.Confidence = true;
            }
            else
            {
                _currentGoalState.AnticipatedDirection = AnticipatedDirection.Up;
                _currentGoalState.Confidence = false;
            }
        }

        protected override void Maximize()
        {
            if (_currentGoalState.Value == _currentGoalState.FocalValue
               || _currentGoalState.Value > _currentGoalState.PriorValue
                    && _currentGoalState.DiffCurrentAndPrior >= _currentGoalState.DiffPriorAndTwicePrior)
            {
                _currentGoalState.AnticipatedDirection = AnticipatedDirection.Stay;
                _currentGoalState.Confidence = true;
            }
            else
            {
                _currentGoalState.AnticipatedDirection = AnticipatedDirection.Up;
                _currentGoalState.Confidence = false;
            }
        }

        protected override void Minimize()
        {
            if (_currentGoalState.Value == _currentGoalState.FocalValue
                || _currentGoalState.Value < _currentGoalState.PriorValue 
                    && _currentGoalState.PriorValue - _currentGoalState.Value 
                        >= _currentGoalState.TwicePriorValue - _currentGoalState.PriorValue)
            {
                _currentGoalState.AnticipatedDirection = AnticipatedDirection.Stay;
                _currentGoalState.Confidence = true;
            }
            else
            {
                _currentGoalState.AnticipatedDirection = AnticipatedDirection.Down;
                _currentGoalState.Confidence = false;
            }
        }

        protected override void MaintainAtValue()
        {
            if (_currentGoalState.Value == _currentGoalState.FocalValue
                || Math.Abs(_currentGoalState.Value - _currentGoalState.FocalValue)
                    < Math.Abs(_currentGoalState.PriorValue - _currentGoalState.PriorFocalValue))
            {
                _currentGoalState.AnticipatedDirection = AnticipatedDirection.Stay;
                _currentGoalState.Confidence = true;
            }
            else
            {
                _currentGoalState.AnticipatedDirection = _currentGoalState.DiffCurrentAndFocal > 0
                    ? AnticipatedDirection.Down
                    : AnticipatedDirection.Up;
                _currentGoalState.Confidence = false;
            }
        }

        #endregion


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
                _currentGoal = goal;
                _currentGoalState = currentIterationAgentState.GoalStates[goal];
                _currentGoalState.Value = agent[goal.ReferenceVariable];
                var previousGoalState = previousIterationAgentState.GoalStates[goal];
                _currentGoalState.PriorValue = previousGoalState.Value;
                _currentGoalState.TwicePriorValue = previousGoalState.PriorValue;
                if (goal.ChangeFocalValueOnPrevious)
                {
                    double reductionPercent = 1;
                    if (goal.ReductionPercent > 0d)
                        reductionPercent = goal.ReductionPercent;
                    _currentGoalState.FocalValue = reductionPercent * _currentGoalState.PriorValue;
                }
                if (!string.IsNullOrEmpty(goal.FocalValueReferenceVariable))
                    _currentGoalState.FocalValue = agent[goal.FocalValueReferenceVariable];
                _currentGoalState.DiffCurrentAndFocal = _currentGoalState.Value - _currentGoalState.FocalValue;
                _currentGoalState.DiffPriorAndFocal = _currentGoalState.PriorValue - _currentGoalState.FocalValue;
                _currentGoalState.DiffCurrentAndPrior = _currentGoalState.Value - _currentGoalState.PriorValue;
                _currentGoalState.DiffPriorAndTwicePrior = _currentGoalState.PriorValue - previousGoalState.PriorValue;

                var anticipatedInfluence = goal.IsCumulative
                    ? _currentGoalState.Value - _currentGoalState.PriorValue
                    : _currentGoalState.Value;

                _currentGoalState.AnticipatedInfluenceValue = anticipatedInfluence;

                //finds activated decision option for each site
                var activatedInPriorIteration =
                    previousIterationAgentState.DecisionOptionHistories.SelectMany(rh => rh.Value.Activated);

                //update anticipated influences of found decision option
                activatedInPriorIteration.ForEach(r =>
                {
                    agent.AnticipationInfluence[r][goal] = anticipatedInfluence;
                });

                SpecificLogic(goal.Tendency);
            }

            //return SortByImportance(agent, currentIterationAgentState.GoalsState).ToArray();
        }
    }
}
