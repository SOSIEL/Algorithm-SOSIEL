using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
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
