// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

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
