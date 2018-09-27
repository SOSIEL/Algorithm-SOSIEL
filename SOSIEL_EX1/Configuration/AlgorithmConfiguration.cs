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
