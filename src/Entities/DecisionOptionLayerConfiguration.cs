// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;
using System.Collections.Generic;

using SOSIEL.Enums;

namespace SOSIEL.Entities
{
    public sealed class DecisionOptionLayerConfiguration
    {
        public bool Modifiable { get; set; }

        public int MaxNumberOfDecisionOptions { get; set; }

        public double[] ConsequentValueInterval { get; set; }

        public int ConsequentPrecisionDigitsAfterDecimalPoint { get; set; }

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
        public double MinValue(IAgent agent)
        {
            if(string.IsNullOrEmpty(MinConsequentReference) == false)
            {
                return (double)agent[MinConsequentReference];
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
        public double MaxValue(IAgent agent)
        {
            if (string.IsNullOrEmpty(MaxConsequentReference) == false)
            {
                return (double)agent[MaxConsequentReference];
            }
            else
            {
                return ConsequentValueInterval[1];
            }
        }
    }
}
