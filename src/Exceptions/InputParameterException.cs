// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

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
