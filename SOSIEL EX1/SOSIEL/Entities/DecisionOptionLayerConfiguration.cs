using System;
using System.Collections.Generic;
using SOSIEL.Enums;

namespace SOSIEL.Entities
{
    public sealed class DecisionOptionLayerConfiguration
    {
        public bool Modifiable { get; set; }

        public int MaxNumberOfDecisionOptions { get; set; }

        public int[] ConsequentValueInterval { get; set; }

        public Dictionary<string, string> ConsequentRelationshipSign { get; set; }

        public static ConsequentRelationship ConvertSign(string sign)
        {
            switch (sign)
            {
                case "+":
                    return ConsequentRelationship.Positive;
                case "-":
                    return ConsequentRelationship.Negative;

                default:
                    throw new Exception("Unknown consequent relationship. See configuration.");
            }
        }

        public string MinConsequentReference { get; set; }

        public string MaxConsequentReference { get; set; }

        public DecisionOptionLayerConfiguration()
        {
            Modifiable = false;
            MaxNumberOfDecisionOptions = 10;
        }

        /// <summary>
        /// Gets min consequent value
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public int MinValue(IAgent agent)
        {
            if(string.IsNullOrEmpty(MinConsequentReference) == false)
            {
                return (int)agent[MinConsequentReference];
            }
            else
            {
                return ConsequentValueInterval[0];
            }
        }

        /// <summary>
        /// Gets max consequent value
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public int MaxValue(IAgent agent)
        {
            if (string.IsNullOrEmpty(MaxConsequentReference) == false)
            {
                return (int)agent[MaxConsequentReference];
            }
            else
            {
                return ConsequentValueInterval[1];
            }
        }
    }
}
