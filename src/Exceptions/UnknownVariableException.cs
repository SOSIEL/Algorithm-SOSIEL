// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;

namespace SOSIEL.Exceptions
{
    public class UnknownVariableException : Exception
    {
        public string VariableName { get; private set; }
        public string AgentId { get; private set; }

        public UnknownVariableException(string variableName, string agentId, bool isForAgent = true) :
            base(
                isForAgent
                ? $"Variable {variableName} is not defined for the agent {agentId}"
                : $"Variable {variableName} is not defined for the agent archetype {agentId}"
            )
        {
            VariableName = variableName;
            AgentId = agentId;
        }
    }
}
