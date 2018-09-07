// Copyright 2018 Garry Sotnik
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this software except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 
﻿ 
using Common.Algorithm;
using SOSIEL_EX1;
using SOSIEL_EX1.Configuration;

namespace Factory
{
    public static class AlgorithmFactory
    {
        public static IAlgorithm Create(string path)
        {
            ConfigurationModel algorithmConfig = ConfigurationParser.ParseConfiguration(path);

            return new Algorithm(algorithmConfig);
        }
    }
}

