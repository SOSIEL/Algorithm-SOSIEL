// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using System;
using System.Collections.Generic;

namespace SOSIEL.Helpers
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
