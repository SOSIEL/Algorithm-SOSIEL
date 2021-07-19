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
///   a degree of uncertainty.

using System;
using System.Collections.Generic;
using System.Linq;

using NLog;

using SOSIEL.Entities;
using SOSIEL.Helpers;

namespace SOSIEL.Processes
{
    /// <summary>
    /// Goal selecting process implementation.
    /// </summary>
    public class GoalSelecting
    {
        private static Logger _logger = LogHelper.GetLogger();

        /// <summary>
        /// Sorts goals by importance
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="goals"></param>
        /// <returns></returns>
        public Goal[] SortByImportance(IAgent agent, Dictionary<Goal, GoalState> goals)
        {
            var goalCount = goals.Count;
            if (_logger.IsDebugEnabled)
                _logger.Debug($"GoalSelecting.SortByImportance: agent={agent.Id} goals.Count={goalCount}");

            if (goalCount == 0)
            {
                throw new ArgumentException(
                    $"Goal selecting can't run for the agent {agent.Id}, because it doesn't have any goals");
            }

            var result = new Goal[goalCount];

            if (goals.Count > 1)
            {
                var v = new List<Goal>(100);
                goals.ForEach(kvp =>
                {
                    int n = (int)Math.Round(kvp.Value.AdjustedImportance * 100);
                    for (int i = 0; i < n; i++) v.Add(kvp.Key);
                });

                var index = 0;
                var importantGoalCount = goals.Where(kvp => kvp.Value.Importance > 0).Count();
                while (v.Count > 0 && index < importantGoalCount)
                {
                    var nextGoal = v.ChooseRandomElement();
                    result[index++] = nextGoal;
                    v.RemoveAll(o => o == nextGoal);
                }

                foreach (var otherGoal in goals.Where(kvp => (int)Math.Round(kvp.Value.AdjustedImportance * 100) == 0)
                                          .OrderByDescending(kvp => kvp.Key.RankingEnabled).Select(kvp => kvp.Key))
                {
                    result[index++] = otherGoal;
                }
            }
            else
            {
                result[0] = goals.Keys.First();
            }
            return result;
        }
    }
}
