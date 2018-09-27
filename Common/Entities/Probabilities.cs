using System;
using System.Collections.Generic;

namespace Common.Entities
{
    /// <summary>
    /// Collection of probability tables
    /// </summary>
    public class Probabilities
    {
        private readonly Dictionary<string, dynamic> _probabilityTables = new Dictionary<string, dynamic>();

        public void AddProbabilityTable<T>(string name, ProbabilityTable<T> table)
        {
            _probabilityTables.Add(name, table);
        }

        public ProbabilityTable<T> GetProbabilityTable<T>(string name)
        {
            dynamic table;

            if (!_probabilityTables.TryGetValue(name, out table))
            {
                throw new ArgumentException("Cannot found probability table by name:" + name);
            }

            return table;
        }
    }
}
