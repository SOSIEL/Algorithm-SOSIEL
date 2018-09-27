using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Enums;

    public sealed class GoalState
    {
        public double Value { get; set; }

        public double PriorValue { get; private set; }

        public double FocalValue { get; set; }

        public double DiffCurrentAndFocal { get; set; }

        public double DiffPriorAndFocal { get; set; }

        public double DiffCurrentAndPrior { get; set; }

        public double DiffPriorAndTwicePrior { get; set; }

        public double AnticipatedInfluenceValue { get; set; }


        public double Importance { get; set; }

        public double AdjustedImportance { get; set; }
        
        public bool Confidence { get; set; }

        public AnticipatedDirection AnticipatedDirection { get; set; }


        public GoalState(double value, double focalValue, double importance)
        {
            //value will be changed in AL
            Value = value;
            PriorValue = value;

            FocalValue = focalValue;
            Importance = importance;
            Confidence = true;
        }


        /// <summary>
        /// Creates goal state for next iteration. Current goal value, focal goal value and importance are copied to new instance.
        /// </summary>
        /// <returns></returns>
        public GoalState CreateForNextIteration()
        {
            return new GoalState(Value, FocalValue, Importance);
        }

    }
}
