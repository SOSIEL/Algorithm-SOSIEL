// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using System;

namespace SOSIEL.Exceptions
{
    public class SosielAlgorithmException : Exception
    {
        public SosielAlgorithmException(string message)
            : base(message) { }
    }
}
