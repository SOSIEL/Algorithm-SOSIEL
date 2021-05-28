// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;

namespace SOSIEL.Exceptions
{
    public class InputParameterException: Exception
    {
        public string Parameter { get; private set; }

        public InputParameterException(string parameter, string message):
            base($"Invalid value of the parameter '{parameter}': {message}")
        {
            Parameter = parameter;
        }
    }
}
