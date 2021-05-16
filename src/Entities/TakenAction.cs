// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

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
