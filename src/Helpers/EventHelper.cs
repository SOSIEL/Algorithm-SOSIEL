// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using SOSIEL.Entities;
using SOSIEL.Randoms;

namespace SOSIEL.Helpers
{
    public static class EventHelper
    {
        private const int _multiplexer = 1000;

        public static bool IsVariableSpecificEventOccur<T>(this ProbabilityTable<T> probabilityTable, T value)
        {
            var probability = probabilityTable.GetProbability(value);
            return IsEventOccur(probability);
        }

        public static bool IsEventOccur(double probability)
        {
            var adjustedProbability = probability * _multiplexer;
            var randomValue = LinearUniformRandom.GetInstance.Next(1, _multiplexer + 1);
            return randomValue <= adjustedProbability;
        }
    }
}
