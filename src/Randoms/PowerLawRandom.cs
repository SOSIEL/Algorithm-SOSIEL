// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

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
