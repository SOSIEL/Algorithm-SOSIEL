using System;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Helpers;

namespace Common.Processes
{
    /// <summary>
    /// Goal selecting process implementation.
    /// </summary>
    public class GoalSelecting
    {
        /// <summary>
        /// Sorts goals by importance 
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="goals"></param>
        /// <returns></returns>
        public IEnumerable<Goal> SortByImportance(IAgent agent, Dictionary<Goal, GoalState> goals)
        {
            if (goals.Count > 1)
            {
                var importantGoals = goals.Where(kvp => kvp.Value.Importance > 0).ToArray();

                List<Goal> vector = new List<Goal>(100);

                goals.ForEach(kvp =>
                {
                    int numberOfInsertions = (int)Math.Round(kvp.Value.AdjustedImportance * 100);

                    for (int i = 0; i < numberOfInsertions; i++) { vector.Add(kvp.Key); }
                });

                for (int i = 0; i < importantGoals.Length && vector.Count > 0; i++)
                {
                    Goal nextGoal = vector.RandomizeOne();

                    vector.RemoveAll(o => o == nextGoal);


                    yield return nextGoal;
                }

                Goal[] otherGoals = goals.Where(kvp => (int)Math.Round(kvp.Value.AdjustedImportance * 100) == 0)
                    .OrderByDescending(kvp => kvp.Key.RankingEnabled).Select(kvp => kvp.Key).ToArray();

                foreach (Goal goal in otherGoals)
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