using System.Collections.Generic;

namespace SOSIEL.Entities
{
    public class MentalModelConfiguration
    {
        public string[] AssociatedWith { get; private set; }

        public bool IsSequential { get; private set; }

        public Dictionary<string, DecisionOptionLayerConfiguration> Layer { get; private set; }
    }
}
