// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

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
