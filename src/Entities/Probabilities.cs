// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;
using System.Collections.Generic;

namespace SOSIEL.Entities
{
    /// <summary>
    /// Collection of probability tables
    /// </summary>
    public class Probabilities
    {
        private readonly Dictionary<string, dynamic> _probabilityTables = new Dictionary<string, dynamic>();
        private readonly Dictionary<string, dynamic> _extendedTables = new Dictionary<string, dynamic>();

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

        public ExtendedProbabilityTable<T> GetExtendedProbabilityTable<T>(string name)
        {
            dynamic table;

            if (!_extendedTables.TryGetValue(name, out table))
            {
                dynamic notExtended;

                if (!_probabilityTables.TryGetValue(name, out notExtended))
                    throw new ArgumentException("Cannot found probability table by name:" + name);

                ProbabilityTable<T> archetype = (ProbabilityTable<T>) notExtended;

                table = new ExtendedProbabilityTable<T>(archetype);
                _extendedTables.Add(name, table);
            }

            return table;
        }
    }
}
