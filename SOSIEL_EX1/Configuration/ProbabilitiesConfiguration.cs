// Copyright 2018 Garry Sotnik
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this software except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 

using System.Collections.Generic;
using Newtonsoft.Json;

namespace SOSIEL_EX1.Configuration
{
    /// <summary>
    /// Probabilities configuration model. Used to parse section "ProbabilitiesConfiguration".
    /// </summary>
    public class ProbabilitiesConfiguration
    {
        [JsonRequired]
        public string Variable { get; set; }

        [JsonRequired]
        public string FilePath { get; set; }

        [JsonRequired]
        public string VariableType { get; set; }

        public bool WithHeader { get; set; }
    }
}