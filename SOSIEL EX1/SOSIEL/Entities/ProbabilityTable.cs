using System.Collections.Generic;

namespace SOSIEL.Entities
{
    public class ProbabilityTable<T>
    {
        private Dictionary<T, double> _probabilityTable;

        public ICollection<T> Keys { get; }

        public ProbabilityTable(Dictionary<T, double> pairs)
        {
            _probabilityTable = new Dictionary<T, double>(pairs);
            Keys = _probabilityTable.Keys;
        }

        public double GetProbability(T value)
        {
            double probability;
            _probabilityTable.TryGetValue(value, out probability);
            return probability;
        }
    }
}