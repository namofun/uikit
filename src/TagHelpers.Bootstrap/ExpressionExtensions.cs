using System.Collections.Generic;

namespace System.Linq.Expressions
{
    /// <summary>
    /// Provide extensions to LINQ Expressions.
    /// </summary>
    internal static class ExpressionExtensions
    {
        internal class ParameterReplaceVisitor : ExpressionVisitor
        {
            public ParameterReplaceVisitor(params (ParameterExpression, Expression)[] changes)
            {
                Changes = changes.ToDictionary(k => k.Item1, k => k.Item2);
            }

            public Dictionary<ParameterExpression, Expression> Changes { get; }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return Changes.GetValueOrDefault(node) ?? node;
            }
        }

        internal static Expression ReplaceWith(this Expression self, ParameterExpression before, Expression after)
        {
            return new ParameterReplaceVisitor((before, after)).Visit(self);
        }

        private class ParameterExpressionHolder<T>
        {
            public static readonly ParameterExpression Param = Expression.Parameter(typeof(T), "param");
        }

        internal static Expression<Func<T, bool>> CombineOrElse<T>(this IReadOnlyList<Expression<Func<T, bool>>> toBind)
        {
            if (toBind.Count == 1)
            {
                return toBind[0];
            }
            else if (toBind.Count == 0)
            {
                return c => false;
            }
            else
            {
                var param = ParameterExpressionHolder<T>.Param;
                Expression? body = null;

                foreach (var item in toBind)
                {
                    var newBody = ReferenceEquals(item.Parameters[0], param)
                        ? item.Body
                        : item.Body.ReplaceWith(item.Parameters[0], param);
                    body = body == null ? newBody : Expression.OrElse(body, newBody);
                }

                var exp = Expression.Lambda<Func<T, bool>>(body!, param);
                return exp;
            }
        }

        internal static Expression<Func<T, bool>> CombineAndAlso<T>(this IReadOnlyList<Expression<Func<T, bool>>> toBind)
        {
            if (toBind.Count == 1)
            {
                return toBind[0];
            }
            else if (toBind.Count == 0)
            {
                return c => true;
            }
            else
            {
                var param = ParameterExpressionHolder<T>.Param;
                Expression? body = null;

                foreach (var item in toBind)
                {
                    var newBody = ReferenceEquals(item.Parameters[0], param)
                        ? item.Body
                        : item.Body.ReplaceWith(item.Parameters[0], param);
                    body = body == null ? newBody : Expression.AndAlso(body, newBody);
                }

                var exp = Expression.Lambda<Func<T, bool>>(body!, param);
                return exp;
            }
        }
    }
}
