// Copyright 2018 Garry Sotnik
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this software except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 

using Newtonsoft.Json;

namespace SOSIEL_EX1.Configuration
{
    /// <summary>
    /// Goal state configuration model. Used to parse section "InitialState.AgentsState.GoalsState".
    /// </summary>
    public class GoalStateConfiguration
    {
        [JsonRequired]
        public double Importance { get; set; }

        [JsonRequired]
        public double Value { get; set; }

        public bool Randomness { get; set; }

        public double RandomFrom { get; set; }

        public double RandomTo { get; set; }

        public string BasedOn { get; set; }
    }
}
