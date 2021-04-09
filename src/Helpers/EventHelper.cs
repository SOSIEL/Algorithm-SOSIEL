/// Name: EventHelper.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik

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
