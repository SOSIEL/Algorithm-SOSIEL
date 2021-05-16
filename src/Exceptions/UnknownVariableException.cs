// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

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
