/// Name: PowerLawRandom.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik

using System;

namespace SOSIEL.Randoms
{
    public class PowerLawRandom
    {
        private static PowerLawRandom _random;

        double _power;

        public int Next(double min, double max)
        {
            Random r = LinearUniformRandom.GetInstance;

            var x = r.NextDouble();

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
