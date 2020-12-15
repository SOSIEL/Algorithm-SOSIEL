/// Name: ExtendedProbabilityTableRandomHelper.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik

using System;
using SOSIEL.Entities;
using SOSIEL.Randoms;

namespace SOSIEL.Helpers
{
    /// <summary>
    /// Probability table random helper
    /// </summary>
    public static class ExtendedProbabilityTableRandomHelper
    {
        /// <summary>
        /// Gets the random value within a specified range, except max value;
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="isReversed">If true, reversed probability table will be used.</param>
        /// <returns></returns>
        public static double GetRandomValue(this ExtendedProbabilityTable<int> table, double min, double max, bool isReversed)
        {
            int batchCount = table.ValueCount;
            double batchSize = Math.Abs(max - min) / batchCount;

            double r1 = LinearUniformRandom.GetInstance.NextDouble();

            int batch = table.GetValueByCumulative(r1);

            double batchMin = min + (batch - 1) * batchSize;

            double r2 = LinearUniformRandom.GetInstance.NextDouble();

            double randomValue = batchMin + batchSize * r2;

            if (isReversed)
                randomValue = min + max - randomValue;

            return randomValue;
        }
    }
}
