using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Configuration
{
    using Enums;
    using Exceptions;

    /// <summary>
    /// Processes configuration. Used in algorithm for managing cognitive processes.
    /// </summary>
    public class ProcessesConfiguration
    {
        public bool AgentRandomizationEnabled { get; set; }

        public bool AnticipatoryLearningEnabled { get; set; }

        public bool CounterfactualThinkingEnabled { get; set; }

        public bool InnovationEnabled { get; set; }

        public bool SocialLearningEnabled { get; set; }

        public bool DecisionOptionSelectionEnabled { get; set; }

        public bool DecisionOptionSelectionPart2Enabled { get; set; }

        public bool ActionTakingEnabled { get; set; }

        public bool ReproductionEnabled { get; set; }

        public bool AgentsDeactivationEnabled { get; set; }

        public bool AlgorithmStopIfAllAgentsSelectDoNothing { get; set; }

        public bool ChangePopulationEnabled { get; set; }

        public bool UseDemographicProcesses { get; set; }

        /// <summary>
        /// Create processes configuration for specific cognitive level
        /// </summary>
        /// <param name="cognitiveLevel"></param>
        /// <returns></returns>
        public static ProcessesConfiguration GetProcessesConfiguration(CognitiveLevel cognitiveLevel)
        {
            switch (cognitiveLevel)
            {
                case CognitiveLevel.CL1:
                    return new ProcessesConfiguration
                    {
                        ActionTakingEnabled = true,
                        DecisionOptionSelectionEnabled = true,
                        AgentRandomizationEnabled = true,
                    };
                case CognitiveLevel.CL2:
                    return new ProcessesConfiguration
                    {
                        ActionTakingEnabled = true,
                        AnticipatoryLearningEnabled = true,
                        DecisionOptionSelectionEnabled = true,
                        AgentRandomizationEnabled = true
                    };
                case CognitiveLevel.CL3:
                    return new ProcessesConfiguration
                    {
                        ActionTakingEnabled = true,
                        AnticipatoryLearningEnabled = true,
                        DecisionOptionSelectionEnabled = true,
                        DecisionOptionSelectionPart2Enabled = true,
                        SocialLearningEnabled = true,
                        AgentRandomizationEnabled = true,
                    };
                case CognitiveLevel.CL4:
                    return new ProcessesConfiguration
                    {
                        ActionTakingEnabled = true,
                        AnticipatoryLearningEnabled = true,
                        DecisionOptionSelectionEnabled = true,
                        DecisionOptionSelectionPart2Enabled = true,
                        SocialLearningEnabled = true,
                        CounterfactualThinkingEnabled = true,
                        InnovationEnabled = true,
                        ReproductionEnabled = true,
                        AgentRandomizationEnabled = true
                    };

                default:
                    throw new SosielAlgorithmException("Unknown cognitive level");
            }
        }
    }


}
