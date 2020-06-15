using Newtonsoft.Json;
using SOSIEL.Configuration;

namespace ModelLuhy.Configuration
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
