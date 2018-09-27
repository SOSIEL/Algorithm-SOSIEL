using System;
using System.Linq.Expressions;
using Microsoft.CSharp.RuntimeBinder;

namespace Common.Helpers
{
    using Algorithm;

    public class AntecedentBuilder
    {
        static ExpressionType GetExpressionType(string inequalitySign)
        {
            switch (inequalitySign)
            {
                case ">":
                    return ExpressionType.GreaterThan;
                case ">=":
                    return ExpressionType.GreaterThanOrEqual;
                case "<":
                    return ExpressionType.LessThan;
                case "<=":
                    return ExpressionType.LessThanOrEqual;
                case "==":
                    return ExpressionType.Equal;

                default:
                    throw new ArgumentException(string.Format("Unsupported antecedent condition: {0}", inequalitySign));
            }
        }

        /// <summary>
        /// Creates expression tree for variable comparison
        /// </summary>
        /// <param name="inequalitySign"></param>
        /// <returns></returns>
        public static Func<dynamic, dynamic, dynamic> Build(string inequalitySign)
        {
            ParameterExpression x = Expression.Parameter(typeof(object), "x");
            ParameterExpression y = Expression.Parameter(typeof(object), "y");
            var binder = Binder.BinaryOperation(
                CSharpBinderFlags.None, GetExpressionType(inequalitySign), typeof(IAlgorithm),
                new CSharpArgumentInfo[] {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                });
            Func<dynamic, dynamic, dynamic> lambda =
                Expression.Lambda<Func<object, object, object>>(Expression.Dynamic(binder, typeof(object), x, y), new[] { x, y }).Compile();

            return lambda;
        }

    }
}
