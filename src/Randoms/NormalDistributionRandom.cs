// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using System;

namespace SOSIEL.Randoms
{
    public class NormalDistributionRandom
    {
        private static NormalDistributionRandom _random;

        double _mean;
        double _stdDev;

        public double Next()
        {
            return Next(_mean, _stdDev);
        }


        public double Next(double mean, double stdDev)
        {
            var r = LinearUniformRandom.GetInstance;
            double u1 = 1 - r.NextDouble();
            double u2 = 1 - r.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mean + stdDev * randStdNormal;
        }

        public static NormalDistributionRandom GetInstance
        {
            get
            {
                if (_random == null)
                    _random = new NormalDistributionRandom(0.3, 0.3);
                return _random;
            }
        }

        private NormalDistributionRandom(double mean, double stdDev)
        {
            _mean = mean;
            _stdDev = stdDev;
        }
    }
}
