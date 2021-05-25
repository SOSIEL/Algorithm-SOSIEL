// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;

namespace SOSIEL.Randoms
{
    public class PowerLawRandom
    {
        private static PowerLawRandom _instance = new PowerLawRandom(3);

        private readonly double _power;

        public static PowerLawRandom Instance { get => _instance; }

        private PowerLawRandom(int powerOfDistribution)
        {
            _power = powerOfDistribution;
        }

        public int Next(double min, double max)
        {
            var x = LinearUniformRandom.Instance.NextDouble();
            return (int)Math.Pow((Math.Pow(max, (_power + 1)) - Math.Pow(min, (_power + 1))) 
                * x + Math.Pow(min, (_power + 1)), (1 / (_power + 1)));
        }
    }
}
