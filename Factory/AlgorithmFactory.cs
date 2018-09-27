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

