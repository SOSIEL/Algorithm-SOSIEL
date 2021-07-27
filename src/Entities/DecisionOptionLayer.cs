// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;
using System.Collections.Generic;

using SOSIEL.Helpers;

namespace SOSIEL.Entities
{
    public class DecisionOptionLayer: IComparable<DecisionOptionLayer>
    {
        int _nextDecisionOptionId = 1;
        public int LayerId { get; set; }

        public MentalModel ParentMentalModel { get; set; }

        public DecisionOptionLayerConfiguration Configuration { get; private set; }

        public List<DecisionOption> DecisionOptions { get; private set; }

        public DecisionOptionLayer(DecisionOptionLayerConfiguration configuration)
        {
            DecisionOptions = new List<DecisionOption>(configuration.MaxNumberOfDecisionOptions);
            Configuration = configuration;
        }

        public DecisionOptionLayer(
            DecisionOptionLayerConfiguration parameters, IEnumerable<DecisionOption> decisionOptions)
            : this(parameters)
        {
            decisionOptions.ForEach(r => Add(r));
        }

        /// <summary>
        /// Adds decision option to the decision option set layer.
        /// </summary>
        /// <param name="decisionOption"></param>
        public void Add(DecisionOption decisionOption)
        {
            decisionOption.Id = _nextDecisionOptionId++;
            decisionOption.ParentLayer = this;
            DecisionOptions.Add(decisionOption);
        }

        /// <summary>
        /// Removes decision option from decision option set layer.
        /// </summary>
        /// <param name="decisionOption"></param>
        public void Remove(DecisionOption decisionOption)
        {
            decisionOption.ParentLayer = null;
            DecisionOptions.Remove(decisionOption);
        }

        public int CompareTo(DecisionOptionLayer other)
        {
            return this == other ? 0 : other.LayerId > LayerId ? -1 : 1;
        }
    }
}
