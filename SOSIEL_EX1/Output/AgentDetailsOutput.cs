// Copyright 2018 Garry Sotnik
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this software except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 

namespace SOSIEL_EX1.Output
{
    // Class declaration
    public class AgentDetailsOutput
    {
        // Field definition
        public const string FileName = "{0}_details.csv";
	
	// Property definition
        public int Iteration { get; set; }

	// Property definition
        public string AgentId { get; set; }

	// Property definition
        public int Age { get; set; }

	// Property definition
        public bool IsAlive { get; set; }

	// Property definition
        public int NumberOfDO { get; set; }

	// Property definition
        public double Income { get; set; }

	// Property definition
        public double Expenses { get; set; }

	// Property definition
        public double Savings { get; set; }
	
	// Property definition
	public string ChosenDecisionOption { get; set; }
    }
}
