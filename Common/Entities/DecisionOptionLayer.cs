using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Entities
{
    using Helpers;   

    public class DecisionOptionLayer: IComparable<DecisionOptionLayer>
    {
        int indexer = 0;
        public int PositionNumber { get; set; }

        public MentalModel Set { get; set; }

        public DecisionOptionLayerConfiguration LayerConfiguration { get; private set; }

        public List<DecisionOption> DecisionOptions { get; private set; }

        public DecisionOptionLayer(DecisionOptionLayerConfiguration configuration)
        {
            DecisionOptions = new List<DecisionOption>(configuration.MaxNumberOfDecisionOptions);
            LayerConfiguration = configuration;
        }

        public DecisionOptionLayer(DecisionOptionLayerConfiguration parameters, IEnumerable<DecisionOption> decisionOptions) : this(parameters)
        {
            decisionOptions.ForEach(r => Add(r));
        }

        /// <summary>
        /// Adds decision option to the decision option set layer.
        /// </summary>
        /// <param name="decisionOption"></param>
        public void Add(DecisionOption decisionOption)
        {
            indexer++;
            decisionOption.PositionNumber = indexer;
            decisionOption.Layer = this;
           
            DecisionOptions.Add(decisionOption);
        }

        /// <summary>
        /// Removes decision option from decision option set layer.
        /// </summary>
        /// <param name="decisionOption"></param>
        public void Remove(DecisionOption decisionOption)
        {
            decisionOption.Layer = null;

            DecisionOptions.Remove(decisionOption);
        }

        public int CompareTo(DecisionOptionLayer other)
        {
            return this == other ? 0 : other.PositionNumber > PositionNumber ? -1 : 1;
        }
    }
}
