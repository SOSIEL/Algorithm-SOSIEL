// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

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
