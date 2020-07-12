/// Name: ReflectionHelper.cs
/// Description:
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
/// Copyright: Garry Sotnik

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

            if (method == null) throw new NullReferenceException(string.Format("Method {0} hasn't benn found on type {1}", methodName, type.Name));

            return method.MakeGenericMethod(passedType);
        }
    }
}
