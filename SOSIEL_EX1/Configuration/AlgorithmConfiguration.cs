// Copyright 2018 Garry Sotnik
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this software except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 

using Common.Configuration;
using Newtonsoft.Json;

namespace SOSIEL_EX1.Configuration
{
    /// <summary>
    /// Algorithm configuration model. Used to parse section "AlgorithmConfiguration".
    /// </summary>
    public class AlgorithmConfiguration
    {
        [JsonRequired]
        public int NumberOfIterations { get; set; }

        public bool UseDimographicProcesses { get; set; }

        public DemographicProcessesConfiguration DemographicConfiguration { get; set; }

        public ProbabilitiesConfiguration[] ProbabilitiesConfiguration { get; set; }
    }
}
