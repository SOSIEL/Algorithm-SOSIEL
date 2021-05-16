// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using System;
using System.Linq;

using SOSIEL.Entities;
using SOSIEL.Randoms;

namespace SOSIEL.Helpers
{
    /// <summary>
    /// Probability table random helper
    /// </summary>
    public static class ProbabilityTableRandomHelper
    {
        private const double _batchCount = 10;
        private const double _probabilityMultiplier = 1000;

        /// <summary>
        /// Gets the random value within a specified range, except max value;
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="isReversed">if set to <c>true</c> [isReversed].</param>
        /// <returns></returns>
        public static int GetRandomValue(this ProbabilityTable<int> table, int min, int max, bool isReversed)
        {
            var potentialValues = Enumerable.Range(min, max - min).ToList();

            var batchSize = potentialValues.Count / _batchCount;

            var potentialValuesTable = potentialValues.GroupBy(v => (v - min) / batchSize + 1)
                .SelectMany(g => g.Select(v => new { Value = v, Probability = table.GetProbability(GetBatchNumber((int)g.Key, isReversed)) })).ToList();

            var randomTable = potentialValuesTable
                .SelectMany(v => Enumerable.Range(1, (int)Math.Ceiling(v.Probability * _probabilityMultiplier))
                    .Select(i => v.Value))
                .ToList();

            var random = LinearUniformRandom.GetInstance.Next(randomTable.Count);

            return randomTable[random];
        }

        private static int GetBatchNumber(int batchNumber, bool isReversed)
        {
            if (isReversed)
            {
                return (int)_batchCount + 1 - batchNumber;
            }

            return batchNumber;
        }
    }
}
