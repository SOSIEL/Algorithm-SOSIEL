using System.Collections;
using System.Collections.Generic;
using Common.Randoms;

namespace Common.Entities
{
    public class ProbabilityTable<T>
    {
        private Dictionary<T, double> _probabilityTable;

        public ProbabilityTable(Dictionary<T, double> pairs)
        {
            _probabilityTable = new Dictionary<T, double>(pairs);
        }

        public double GetProbability(T value)
        {
            double probability;
            _probabilityTable.TryGetValue(value, out probability);
            return probability;
        }
    }
}