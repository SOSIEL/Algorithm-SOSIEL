using System.Collections.Generic;
using Newtonsoft.Json;

namespace ModelLuhy.Configuration
{
    /// <summary>
    /// Agent state configuration model. Used to parse section "InitialState.AgentsState".
    /// </summary>
    public class AgentStateConfiguration
    {
        [JsonRequired]
        public string ArchetypeOfAgent { get; set; }

        [JsonRequired]
        public int NumberOfAgents { get; set; }

        [JsonRequired]
        public Dictionary<string, dynamic> PrivateVariables { get; set; }

        [JsonRequired]
        public Dictionary<string, Dictionary<string, double>> AnticipatedInfluenceState { get; set; }

        public Dictionary<string, Dictionary<string, string>> AnticipatedInfluenceTransform { get; set; }

        [JsonRequired]
        public string[] AssignedDecisionOptions { get; set; }

        [JsonRequired]
        public string[] AssignedGoals { get; set; }

        [JsonRequired]
        public Dictionary<string, GoalStateConfiguration> GoalsState { get; set; }
    }
}
