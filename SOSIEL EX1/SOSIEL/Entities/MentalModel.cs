using System;
using System.Collections.Generic;
using System.Linq;
using SOSIEL.Helpers;

namespace SOSIEL.Entities
{
    public class MentalModel: IComparable<MentalModel>
    {
        int layerIndexer = 0;

        public int PositionNumber { get; set; }
        public List<DecisionOptionLayer> Layers { get; private set; } 

        public Goal[] AssociatedWith { get; private set; }

        private MentalModel(int number, Goal[] associatedGoals)
        {
            PositionNumber = number;
            Layers = new List<DecisionOptionLayer>();
            AssociatedWith = associatedGoals;
        }

        public MentalModel(int number, Goal[] associatedGoals, IEnumerable<DecisionOptionLayer> layers) :this(number, associatedGoals)
        {
            layers.ForEach(l => Add(l));
        }

        /// <summary>
        /// Adds layer to the decision option set.
        /// </summary>
        /// <param name="layer"></param>
        public void Add(DecisionOptionLayer layer)
        {
            layerIndexer++;
            layer.Set = this;
            layer.PositionNumber = layerIndexer;

            Layers.Add(layer);
        }


        public IEnumerable<DecisionOption> AsDecisionOptionEnumerable()
        {
            return Layers.SelectMany(rl => rl.DecisionOptions);
        }

        public int CompareTo(MentalModel other)
        {
            return this == other ? 0 : other.PositionNumber > PositionNumber ? -1 : 1;
        }
    }
}
