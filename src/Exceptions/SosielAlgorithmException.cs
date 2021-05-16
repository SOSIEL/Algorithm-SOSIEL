// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;

namespace SOSIEL.Exceptions
{
    public class SosielAlgorithmException : Exception
    {
        public SosielAlgorithmException(string message)
            : base(message) { }
    }
}
