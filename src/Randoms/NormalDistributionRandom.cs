// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;

namespace SOSIEL.Randoms
{
    public class NormalDistributionRandom
    {
        private static NormalDistributionRandom _instance = new NormalDistributionRandom(0.3, 0.3);

        private readonly double _mean;
        private readonly double _stdDev;

        public static NormalDistributionRandom Instance { get => _instance; }

        private NormalDistributionRandom(double mean, double stdDev)
        {
            _mean = mean;
            _stdDev = stdDev;
        }

        public double Next()
        {
            return Next(_mean, _stdDev);
        }

        public double Next(double mean, double stdDev)
        {
            var r = LinearUniformRandom.Instance;
            var u1 = 1.0 - r.NextDouble();
            var u2 = 1.0 - r.NextDouble();
            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mean + stdDev * randStdNormal;
        }
    }
}
