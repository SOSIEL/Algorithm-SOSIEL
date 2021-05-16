// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;

namespace SOSIEL.Exceptions
{
    public class InputParameterException: Exception
    {
        private readonly string _parameter;
        private readonly string _message;

        public InputParameterException(string parameter, string message)
        {
            _parameter = parameter;
            _message = message;
        }

        public override string ToString()
        {
            return string.Format("Value of {0} is wrong. {1}", _parameter, _message);
        }
    }
}
