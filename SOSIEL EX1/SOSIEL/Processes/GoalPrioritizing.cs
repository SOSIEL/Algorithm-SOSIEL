// Goal prioritizing always follows anticipatory learning. the aim of goal prioritizing is to use
// what was learned during anticipatory learning to reevaluate the importance levels of goals and,
// if necessary, reprioritize them. The process of goal prioritizing has a stabilizing effect on
// agent behavior and has the option of being turned off as a mechanism if its stabilizing effect
// contradicts reference behavior. The process of goal prioritizing consists of the following two
// subprocesses: (a) determine the relative difference between goal value and focal goal value and
// (b) adjust the proportional importance levels of goals respectively. The result of goal prioritizing
// is a reevaluated and, if appropriate, a reprioritized set of proportional importance levels.

using System;
using System.Collections.Generic;
using System.Linq;
using SOSIEL.Entities;
using SOSIEL.Enums;
using SOSIEL.Exceptions;
using SOSIEL.Helpers;

namespace SOSIEL.Processes
{
    /// <summary>
    /// Goal prioritizing process implementation.
    /// </summary>
    public class GoalPrioritizing
    {
        /// <summary>
        /// Prioritizes agent goals.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="goals">The goals.</param>
        public void Prioritize(IAgent agent, Dictionary<Goal, GoalState> goals)
        {
            if (goals.Count > 1)
            {
                var importantGoals = goals.Where(kvp => kvp.Value.Importance > 0).ToArray();

                var noConfidenceGoals = importantGoals.Where(kvp => kvp.Value.Confidence == false).ToArray();

                if (noConfidenceGoals.Length > 0 && agent.Archetype.UseImportanceAdjusting)
                {
                    var noConfidenceProportions = noConfidenceGoals.Select(kvp => new
                    {
                        Proportion = kvp.Value.Importance * CalculateRelativedDifference(agent, kvp.Key, kvp.Value),
                        Goal = kvp.Key
                    }).ToArray();

                    var confidenceGoals = goals.Where(kvp => kvp.Value.Confidence).ToArray();

                    double totalConfidenceUnadjustedProportions = confidenceGoals.Sum(kvp => kvp.Value.Importance);

                    double totalNoConfidenceAdjustedProportions = noConfidenceProportions.Sum(p => p.Proportion);

                    var importanceSum = totalConfidenceUnadjustedProportions + totalNoConfidenceAdjustedProportions;

                    var confidenceProportions = confidenceGoals.Select(kvp => new
                    {
                        Proportion = kvp.Value.AdjustedImportance / importanceSum,
                        Goal = kvp.Key
                    }).ToArray();

                    Enumerable.Concat(noConfidenceProportions, confidenceProportions)
                        .ForEach(p =>
                        {
                            goals[p.Goal].AdjustedImportance = p.Proportion;

                        });
                }
                else
                {
                    goals.ForEach(kvp =>
                    {
                        kvp.Value.AdjustedImportance = kvp.Value.Importance;
                    });
                }
            }
        }

        /// <summary>
        /// Calculates normalized value for goal prioritizing.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="goal"></param>
        /// <param name="goalState"></param>
        /// <returns></returns>
        double CalculateRelativedDifference(IAgent agent, Goal goal, GoalState goalState)
        {
            double goalValue = goalState.Value;

            if (goal.Tendency == GoalTendency.Maximize)
            {
                double focalValue = goalState.PriorValue;
                double maxGoalValue = goalState.GetMaxGoalValue();

                if ((maxGoalValue - focalValue) == 0
                    || ((maxGoalValue - goalValue) / (maxGoalValue - focalValue)) < 0)
                    return 1d;

                double adjustment = (maxGoalValue - goalValue) / (maxGoalValue - focalValue);
                return adjustment;
            }

            if (goal.Tendency == GoalTendency.EqualToOrAboveFocalValue)
            {
                double focalValue = goalState.FocalValue;
                double maxGoalValue = goalState.GetMaxGoalValue();

                if ((maxGoalValue - focalValue) == 0
                    || ((maxGoalValue - goalValue) / (maxGoalValue - focalValue)) < 0)
                    return 1;

                double adjustment = (maxGoalValue - goalValue) / (maxGoalValue - focalValue);
                return adjustment;
            }

            if (goal.Tendency == GoalTendency.Minimize)
            {
                double focalValue = goalState.PriorValue;
                double minGoalValue = goalState.GetMinGoalValue();

                if ((focalValue - minGoalValue) == 0
                    || ((goalValue - minGoalValue) / (focalValue - minGoalValue)) < 0)
                    return 1;

                double adjustment = (goalValue - minGoalValue) / (focalValue - minGoalValue);
                return adjustment;
            }

            if (goal.Tendency == GoalTendency.MaintainAtValue)
            {
                if (Math.Abs(goalState.DiffPriorAndFocal) == 0)
                    return 1;

                double adjustment = Math.Abs(goalState.DiffCurrentAndFocal) / Math.Abs(goalState.DiffPriorAndFocal);
                return adjustment;
            }

            throw new SosielAlgorithmException(
                "Cannot calculate relative difference between goal value and focal goal value for tendency" +
                goal.Tendency);
        }
    }
}