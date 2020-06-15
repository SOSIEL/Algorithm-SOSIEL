namespace ModelLuhy.Output
{
    public class AgentDetailsOutput
    {
        public const string FileName = "{0}_details.csv";

        public int Iteration { get; set; }

        public string AgentId { get; set; }

        public int Age { get; set; }

        public bool IsAlive { get; set; }

        public int NumberOfDO { get; set; }

        public double Income { get; set; }

        public double Expenses { get; set; }

        public double Savings { get; set; }

        public string ChosenDecisionOption { get; set; }
    }
}