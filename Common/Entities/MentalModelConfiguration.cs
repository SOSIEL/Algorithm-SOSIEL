using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Entities
{
    public class MentalModelConfiguration
    {
        public string[] AssociatedWith { get; private set; }

        public bool IsSequential { get; private set; }

        public Dictionary<string, DecisionOptionLayerConfiguration> Layer { get; private set; }
    }
}
