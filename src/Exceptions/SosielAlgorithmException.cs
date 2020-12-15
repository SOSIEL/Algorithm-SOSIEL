/// Name: SosielAlgorithmException.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik

﻿using System;

namespace SOSIEL.Exceptions
{
    public class SosielAlgorithmException : Exception
    {
        public SosielAlgorithmException(string message)
            :base(message) { }
    }
}
