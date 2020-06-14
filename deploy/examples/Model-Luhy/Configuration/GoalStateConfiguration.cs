using Newtonsoft.Json;

namespace ModelLuhy.Configuration
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
