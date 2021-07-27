// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using SOSIEL.Helpers;

namespace SOSIEL.Entities
{
    public class MentalModel: IComparable<MentalModel>
    {
        int _nextLayerId = 1;

        public int ModelId { get; set; }
        public List<DecisionOptionLayer> Layers { get; private set; }

        public Goal[] AssociatedGoals { get; private set; }

        private MentalModel(int number, Goal[] associatedGoals)
        {
            // Debugger.Launch();
            ModelId = number;
            Layers = new List<DecisionOptionLayer>();
            AssociatedGoals = associatedGoals;
        }

        public MentalModel(int number, Goal[] associatedGoals, IEnumerable<DecisionOptionLayer> layers) 
            : this(number, associatedGoals)
        {
            layers.ForEach(l => Add(l));
        }

        /// <summary>
        /// Adds layer to the decision option set.
        /// </summary>
        /// <param name="layer"></param>
        public void Add(DecisionOptionLayer layer)
        {
            layer.ParentMentalModel = this;
            layer.LayerId = _nextLayerId++;
            Layers.Add(layer);
        }


        public IEnumerable<DecisionOption> AsDecisionOptionEnumerable()
        {
            return Layers.SelectMany(rl => rl.DecisionOptions);
        }

        public int CompareTo(MentalModel other)
        {
            return this == other ? 0 : other.ModelId > ModelId ? -1 : 1;
        }
    }
}
