using System.Collections.Generic;

namespace SOSIEL.Entities
{
    public class DecisionOptionsHistory
    {
        public List<DecisionOption> Matched { get; private set; }
        public List<DecisionOption> Activated { get; private set; }

        public List<DecisionOption> Blocked { get; private set; }

        public DecisionOptionsHistory()
        {
            Matched = new List<DecisionOption>();
            Activated = new List<DecisionOption>();

            Blocked = new List<DecisionOption>();
        }

        public DecisionOptionsHistory(IEnumerable<DecisionOption> matched, IEnumerable<DecisionOption> activated) : base()
        {
            Matched.AddRange(matched);
            Activated.AddRange(activated);
        }
    }
}
