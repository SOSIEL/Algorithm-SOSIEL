// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;

namespace SOSIEL.Exceptions
{
    public class UnknownVariableException : Exception
    {
        private readonly string _variableName;

        public UnknownVariableException(string variableName)
        {
            _variableName = variableName;
        }

        public override string ToString()
        {
            return string.Format("{0} wasn't defined for agent. Maybe you forgot to define it in config", _variableName);
        }
    }
}
