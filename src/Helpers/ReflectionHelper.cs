// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using System;
using System.Reflection;

namespace SOSIEL.Helpers
{
    /// <summary>
    /// Reflection helper class
    /// </summary>
    public static class ReflectionHelper
    {
        public static MethodInfo GetGenerecMethod(Type passedType, Type type, string methodName)
        {
            var method = type.GetMethod(methodName);

            if (method == null)
            {
                throw new NullReferenceException(string.Format("Method {0} not found found on type {1}",
                    methodName, type.Name));
            }

            return method.MakeGenericMethod(passedType);
        }
    }
}
