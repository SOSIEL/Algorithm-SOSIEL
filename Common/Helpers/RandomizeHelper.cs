using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Helpers
{
    using Randoms;

    public static class RandomizeHelper
    {
        /// <summary>
        /// Returns one element using linear uniform distribution.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></par
        public static T RandomizeOne<T>(this IEnumerable<T> source)
        {
            return RandomizeOne(source.ToList());
        }

        /// <summary>
        /// Returns one element using linear uniform distribution.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></par
        public static T RandomizeOne<T>(this List<T> source)
        {
            int position = LinearUniformRandom.GetInstance.Next(source.Count);

            return source.Count > 0 ? source[position] : default(T);
        }

        private static IEnumerable<T> RandomizeEnumeration<T>(this IEnumerable<T> original)
        {
            List<T> temp = new List<T>(original);

            while (temp.Count > 0)
            {
                T item = temp[LinearUniformRandom.GetInstance.Next(temp.Count)];

                temp.Remove(item);

                yield return item;
            }
        }


        /// <summary>
        /// Shuffles elements using linear uniform distribution.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="randomize"></param>
        /// <returns></returns>
        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> original, bool randomize = true)
        {
            if (randomize)
            {
                return RandomizeEnumeration(original);
            }
            else
            {
                return original;
            }
        }
    }
}
