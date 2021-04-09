/// Name: LinearUniformRandom.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik

using System;

namespace SOSIEL.Randoms
{
    public sealed class LinearUniformRandom
    {
        private static Random _random = new Random();

        public static Random GetInstance
        {
            get
            {
                return _random;
            }
        }

        private LinearUniformRandom() { }
    }
}
