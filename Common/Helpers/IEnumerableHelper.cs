 using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Helpers
{
    public static class IEnumerableHelper
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach(T obj in enumerable)
            {
                action(obj);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
        {
            int i = 0;

            foreach (T obj in enumerable)
            {
                action(obj, i);
                i++;
            }
        }
    }
}
