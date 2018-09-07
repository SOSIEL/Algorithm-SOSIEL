// Copyright 2018 Garry Sotnik
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this software except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 

using System.Collections.Generic;
using Common.Configuration;
using Common.Entities;
using Newtonsoft.Json;

namespace SOSIEL_EX1.Configuration
{
    /// <summary>
    /// Main configuration model.
    /// </summary>
    public class ConfigurationModel
    {
        [JsonRequired]
        public AlgorithmConfiguration AlgorithmConfiguration { get; set; }

        [JsonRequired]
        public Dictionary<string, AgentPrototype> AgentConfiguration { get; set; }

        [JsonRequired]
        public InitialStateConfiguration InitialState { get; set; }
    }
}
