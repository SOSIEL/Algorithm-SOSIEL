/// Name: SosielAlgorithmException.cs
/// Description:
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
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
