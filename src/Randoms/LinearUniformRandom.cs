/// Name: LinearUniformRandom.cs
/// Description:
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
/// Copyright: Garry Sotnik

using System;

namespace SOSIEL.Randoms
{
    public sealed class LinearUniformRandom
    {
        private static Random random = new Random();

        public static Random GetInstance
        {
            get
            {
                return random;
            }
        }

        private LinearUniformRandom() { }
    }
}
