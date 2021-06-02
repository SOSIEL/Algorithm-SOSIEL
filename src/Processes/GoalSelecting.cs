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
        public IEnumerable<Goal> SortByImportance(IAgent agent, Dictionary<Goal, GoalState> goals)
        {
            if (_logger.IsDebugEnabled) 
                _logger.Debug($"GoalSelecting.SortByImportance: agent={agent.Id}");

            if (goals.Count > 1)
            {
                var importantGoals = goals.Where(kvp => kvp.Value.Importance > 0).ToArray();
                var vector = new List<Goal>(100);

                goals.ForEach(kvp =>
                {
                    int numberOfInsertions = (int)Math.Round((double) (kvp.Value.AdjustedImportance * 100));
                    for (int i = 0; i < numberOfInsertions; i++) { vector.Add(kvp.Key); }
                });

                for (int i = 0; i < importantGoals.Length && vector.Count > 0; i++)
                {
                    var nextGoal = vector.RandomizeOne();
                    vector.RemoveAll(o => o == nextGoal);
                    yield return nextGoal;
                }

                var otherGoals = goals.Where(kvp => (int)Math.Round(kvp.Value.AdjustedImportance * 100) == 0)
                    .OrderByDescending(kvp => kvp.Key.RankingEnabled).Select(kvp => kvp.Key).ToArray();

                foreach (var goal in otherGoals)
                {
                    yield return goal;
                }
            }
            else
            {
                yield return goals.Keys.First();
            }
        }
    }
}
