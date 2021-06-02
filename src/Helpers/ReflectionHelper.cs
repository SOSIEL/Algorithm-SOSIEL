// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

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
