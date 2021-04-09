/// Name: TakenAction.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik

namespace SOSIEL.Entities
{
    public class TakenAction
    {
        public string DecisionOptionId { get; private set; }

        public string VariableName { get; private set; }

        public dynamic Value { get; private set; }

        public TakenAction(string decisionOptionId, string variableName, dynamic value)
        {
            DecisionOptionId = decisionOptionId;
            VariableName = variableName;
            Value = value;
        }
    }
}
