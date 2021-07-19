// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System.Collections.Generic;
using System.Linq;

using SOSIEL.Randoms;

namespace SOSIEL.Helpers
{
    public static class RandomizeHelper
    {
        /// <summary>
        /// Returns one element using linear uniform distribution.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></par
        public static T ChooseRandomElement<T>(this IEnumerable<T> source)
        {
            return ChooseRandomElement(source.ToList());
        }

        /// <summary>
        /// Returns one element using linear uniform distribution.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></par
        public static T ChooseRandomElement<T>(this List<T> source)
        {
            int position = LinearUniformRandom.Instance.Next(source.Count);
            return source.Count > 0 ? source[position] : default(T);
        }

        /// <summary>
        /// Shuffles elements using linear uniform distribution.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <returns></returns>
        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> original)
        {
            var temp = new List<T>(original);
            while (temp.Count > 0)
            {
                var item = temp[LinearUniformRandom.Instance.Next(temp.Count)];
                temp.Remove(item);
                yield return item;
            }
        }
    }
}
