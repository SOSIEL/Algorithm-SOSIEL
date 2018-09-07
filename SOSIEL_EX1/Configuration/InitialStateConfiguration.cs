// Copyright 2018 Garry Sotnik
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this software except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 

using Newtonsoft.Json;

namespace SOSIEL_EX1.Configuration
{
    /// <summary>
    /// Initial state configuration model. Used to parse section "InitialState".
    /// </summary>
    public class InitialStateConfiguration
    {
        [JsonRequired]
        public AgentStateConfiguration[] AgentsState { get; set; }
    }
}
