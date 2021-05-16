// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;

namespace SOSIEL.Randoms
{
    public class PowerLawRandom
    {
        private static PowerLawRandom _random;

        double _power;

        public int Next(double min, double max)
        {
            var x = LinearUniformRandom.GetInstance.NextDouble();
            return (int)Math.Pow((Math.Pow(max, (_power + 1)) - Math.Pow(min, (_power + 1))) 
                * x + Math.Pow(min, (_power + 1)), (1 / (_power + 1)));
        }

        public static PowerLawRandom GetInstance
        {
            get
            {
                if (_random == null)
                    _random = new PowerLawRandom(3);
                return _random;
            }
        }

        private PowerLawRandom(int powerOfDistribution)
        {
            _power = powerOfDistribution;
        }
    }
}
