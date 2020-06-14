using System.Collections.Generic;

namespace SOSIEL.Entities
{
    public class MentalModelConfiguration
    {
        public string Name { get; set; }

        public string[] AssociatedWith { get; set; }

        //public bool IsSequential { get; private set; }

        public Dictionary<string, DecisionOptionLayerConfiguration> Layer { get; set; }
    }
}
