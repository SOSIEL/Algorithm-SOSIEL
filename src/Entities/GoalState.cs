// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using SOSIEL.Enums;

namespace SOSIEL.Entities
{
    public sealed class GoalState
    {
        public Goal Goal { get; private set; }

        public IAgent Agent { get; private set; }

        public double Value { get; set; }

        public double PriorValue { get; set; }

        public double TwicePriorValue { get; set; }

        public double FocalValue { get; set; }
        
        public double PriorFocalValue { get; set; }

        public double DiffCurrentAndFocal { get; set; }

        public double DiffPriorAndFocal { get; set; }

        public double DiffCurrentAndPrior { get; set; }

        public double DiffPriorAndTwicePrior { get; set; }

        public double AnticipatedInfluenceValue { get; set; }

        public double Importance { get; set; }

        public double AdjustedImportance { get; set; }

        public bool Confidence { get; set; }

        public AnticipatedDirection AnticipatedDirection { get; set; }

        public double MinGoalValueStatic { get; set; }

        public double MaxGoalValueStatic { get; set; }

        public string MinGoalValueReference { get; set; }

        public string MaxGoalValueReference { get; set; }

        public GoalState(
            Goal goal, IAgent agent, double value, double focalValue, double importance,
            double minGoalValueStatic, double maxGoalValueStatic,
            string minGoalValueReference, string maxGoalValueReference
        )
        {
            Goal = goal;
            Agent = agent;

            //value will be changed in AL
            Value = value;
            PriorValue = value;

            FocalValue = focalValue;
            PriorFocalValue = focalValue;
            
            Importance = importance;
            AdjustedImportance = importance;

            MinGoalValueStatic = minGoalValueStatic;
            MaxGoalValueStatic = maxGoalValueStatic;

            MinGoalValueReference = minGoalValueReference;
            MaxGoalValueReference = maxGoalValueReference;

            Confidence = true;
        }

        public GoalState(GoalState src)
        {
            Goal = src.Goal;
            Agent = src.Agent;

            Value = src.Value;
            PriorValue = src.PriorValue;

            FocalValue = src.FocalValue;
            PriorFocalValue = src.PriorFocalValue;

            Importance = src.Importance;
            AdjustedImportance = src.AdjustedImportance;

            MinGoalValueStatic = src.MinGoalValueStatic;
            MaxGoalValueStatic = src.MaxGoalValueStatic;

            MinGoalValueReference = src.MinGoalValueReference;
            MaxGoalValueReference = src.MaxGoalValueReference;

            Confidence = src.Confidence;
        }

        /// <summary>
        /// Creates goal state for next iteration.
        /// Current goal value, focal goal value and importance are copied to new instance.
        /// </summary>
        /// <returns></returns>
        public GoalState CreateForNextIteration()
        {
            return CreateCopy(this);
        }

        /// <summary>
        /// Gets the minimum goal value.
        /// </summary>
        /// <returns></returns>
        public double GetMinGoalValue()
        {
            if (string.IsNullOrEmpty(MinGoalValueReference))
                return MinGoalValueStatic;
            return Agent[MinGoalValueReference];
        }

        /// <summary>
        /// Gets the maximum goal value.
        /// </summary>
        /// <returns></returns>
        public double GetMaxGoalValue()
        {
            if (string.IsNullOrEmpty(MaxGoalValueReference))
                return MaxGoalValueStatic;
            return Agent[MaxGoalValueReference];
        }

        /// <summary>
        /// Creates a goal state copy.
        /// Not sure if this is really needed, but preserve for now.
        /// TODO - Remove later (v3.0).
        /// </summary>
        /// <param name="goalState">State of the goal</param>
        /// <returns></returns>
        public static GoalState CreateCopy(GoalState goalState)
        {
            return new GoalState(
                goalState.Goal, goalState.Agent, goalState.Value, goalState.FocalValue,
                goalState.AdjustedImportance, goalState.MinGoalValueStatic, goalState.MaxGoalValueStatic,
                goalState.MinGoalValueReference, goalState.MaxGoalValueReference
            );
        }
    }
}
