using System;

namespace Common.Randoms
{
    public class PowerLawRandom
    {
        private static PowerLawRandom random;

        double power;

        public int Next(double min, double max)
        {
            Random r = LinearUniformRandom.GetInstance;

            var x = r.NextDouble();

            return (int)Math.Pow((Math.Pow(max, (power + 1)) - Math.Pow(min, (power + 1))) * x + Math.Pow(min, (power + 1)), (1 / (power + 1)));
        }

        public static PowerLawRandom GetInstance
        {
            get
            {
                if (random == null)
                    random = new PowerLawRandom(3);

                return random;
            }
        }

        private PowerLawRandom(int powerOfDistribution)
        {
            power = powerOfDistribution;
        }
    }
}
