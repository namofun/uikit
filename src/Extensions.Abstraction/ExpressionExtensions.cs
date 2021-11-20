#nullable enable
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CA = System.Diagnostics.CodeAnalysis;

namespace System.Linq.Expressions
{
    /// <summary>
    /// The extensions for expressions.
    /// </summary>
    [CA.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Object Template")]
    public static class ExpressionExtensions
    {
        /// <summary>
        /// The expression replacing visitor.
        /// </summary>
        private class ReplaceExpressionVisitor : ExpressionVisitor
        {
            /// <summary>
            /// The property for some expressions.
            /// </summary>
            /// <param name="key">The source key.</param>
            /// <returns>The result value.</returns>
            public Expression this[Expression key] { get => Changes[key]; set => Changes[key] = value; }

            /// <summary>
            /// The target changes.
            /// </summary>
            public Dictionary<Expression, Expression> Changes { get; } = new Dictionary<Expression, Expression>();

            /// <summary>
            /// Attach the expression to change from <paramref name="from"/> to <paramref name="to"/>.
            /// </summary>
            /// <param name="from">The source expression.</param>
            /// <param name="to">The target expression.</param>
            /// <returns>The resulting visitor.</returns>
            public ReplaceExpressionVisitor Attach(Expression from, Expression to)
            {
                Changes.Add(from, to);
                return this;
            }

            /// <inheritdoc />
            [return: CA.NotNullIfNotNull("node")]
            public override Expression? Visit(Expression? node)
            {
                return node != null
                    ? Changes.TryGetValue(node, out var exp) ? exp : base.Visit(node) // original expression or new expression
                    : null!; // Static members in a class will make the main expression be null, but not Expression.Constant(null)
            }
        }

        /// <summary>
        /// The parameter type holder class.
        /// </summary>
        /// <typeparam name="T">The targetting type.</typeparam>
        private class ParameterExpressionHolder<T>
        {
            /// <summary>
            /// The parameter expression.
            /// </summary>
            public static readonly ParameterExpression Param = Expression.Parameter(typeof(T), "param");
        }

        /// <summary>
        /// Combine some expressions with anonymous parameter type.
        /// </summary>
        /// <typeparam name="T1">The computed source type.</typeparam>
        /// <typeparam name="T2">The computed source type.</typeparam>
        /// <typeparam name="T3">The computed source type.</typeparam>
        /// <typeparam name="T4">The computed source type.</typeparam>
        /// <param name="expression">The expression to use.</param>
        /// <param name="objectTemplate">The source anonymous object template to use this function.</param>
        /// <param name="selector">The other expression to combine.</param>
        /// <returns>The combined expression.</returns>
        [return: CA.NotNullIfNotNull("expression")]
        public static Expression<Func<T1, T2, T3>>? Combine<T1, T2, T3, T4>(
            this Expression<Func<T4, T2, T3>>? expression,
            T1 objectTemplate,
            Expression<Func<T1, T4>> selector)
        {
            if (expression == null) return null;
            var s1 = selector.Parameters[0];
            var s2 = expression.Parameters[1];
            var newBody = expression.Body.ReplaceWith(expression.Parameters[0], selector.Body);
            return Expression.Lambda<Func<T1, T2, T3>>(newBody, s1, s2);
        }

        /// <summary>
        /// Combine some condition with other.
        /// </summary>
        /// <typeparam name="T">The computed source type.</typeparam>
        /// <param name="expression">The expression to use.</param>
        /// <param name="other">The other expression to combine.</param>
        /// <returns>The combined expression.</returns>
        public static Expression<Func<T, bool>> Combine<T>(
            this Expression<Func<T, bool>>? expression,
            Expression<Func<T, bool>> other)
        {
            if (expression == null) return other;
            var parameter = expression.Parameters.Single();
            var hold1 = expression.Body;
            var hold2 = new ReplaceExpressionVisitor()
                .Attach(other.Parameters[0], parameter)
                .Visit(other.Body);
            var newBody = Expression.AndAlso(hold1, hold2);
            return Expression.Lambda<Func<T, bool>>(newBody, parameter);
        }

        /// <summary>
        /// Combine some condition with other.
        /// </summary>
        /// <typeparam name="T1">The computed source type.</typeparam>
        /// <typeparam name="T2">The computed source type.</typeparam>
        /// <param name="expression">The expression to use.</param>
        /// <param name="other">The other expression to combine.</param>
        /// <returns>The combined expression.</returns>
        public static Expression<Func<T1, T2, bool>> Combine<T1, T2>(
            this Expression<Func<T1, T2, bool>>? expression,
            Expression<Func<T1, T2, bool>> other)
        {
            if (expression == null) return other;
            var para0 = expression.Parameters[0];
            var para1 = expression.Parameters[1];
            var hold1 = expression.Body;
            var hold2 = new ReplaceExpressionVisitor()
                .Attach(other.Parameters[0], para0)
                .Attach(other.Parameters[1], para1)
                .Visit(other.Body);
            var newBody = Expression.AndAlso(hold1, hold2);
            return Expression.Lambda<Func<T1, T2, bool>>(newBody, para0, para1);
        }

        /// <summary>
        /// Combine some expressions with anonymous parameter type.
        /// </summary>
        /// <typeparam name="T1">The placehold 1.</typeparam>
        /// <typeparam name="T2">The placehold 2.</typeparam>
        /// <typeparam name="T3">The resulting placehold 3.</typeparam>
        /// <typeparam name="T4">The targetted placehold 4.</typeparam>
        /// <param name="expression">The expression to change.</param>
        /// <param name="objectTemplate">The source anonymous object template to use this function.</param>
        /// <param name="place1">The placehold 1 selector.</param>
        /// <param name="place2">The placehold 2 selector.</param>
        /// <returns>The compiled object.</returns>
        [return: CA.NotNullIfNotNull("expression")]
        public static Expression<Func<T4, T3>>? Combine<T1, T2, T3, T4>(
            this Expression<Func<T1, T2, T3>>? expression,
            T4 objectTemplate,
            Expression<Func<T4, T1>> place1,
            Expression<Func<T4, T2>> place2)
        {
            if (expression == null) return null;
            var parameter = place1.Parameters.Single();
            var hold1 = place1.Body;
            var hold2 = new ReplaceExpressionVisitor()
                .Attach(place2.Parameters[0], parameter)
                .Visit(place2.Body);
            var newBody = new ReplaceExpressionVisitor()
                .Attach(expression.Parameters[0], hold1)
                .Attach(expression.Parameters[1], hold2)
                .Visit(expression.Body);
            return Expression.Lambda<Func<T4, T3>>(newBody, parameter);
        }

        /// <summary>
        /// Combine some expressions with anonymous parameter type.
        /// </summary>
        /// <typeparam name="T1">The placehold 1.</typeparam>
        /// <typeparam name="T2">The placehold 2.</typeparam>
        /// <typeparam name="T3">The placehold 3.</typeparam>
        /// <typeparam name="T4">The resulting placehold 4.</typeparam>
        /// <typeparam name="T5">The targetted placehold 5.</typeparam>
        /// <param name="expression">The expression to change.</param>
        /// <param name="objectTemplate">The source anonymous object template to use this function.</param>
        /// <param name="place1">The placehold 1 selector.</param>
        /// <param name="place2">The placehold 2 selector.</param>
        /// <param name="place3">The placehold 3 selector.</param>
        /// <returns>The compiled object.</returns>
        [return: CA.NotNullIfNotNull("expression")]
        public static Expression<Func<T5, T4>>? Combine<T1, T2, T3, T4, T5>(
            this Expression<Func<T1, T2, T3, T4>>? expression,
            T5 objectTemplate,
            Expression<Func<T5, T1>> place1,
            Expression<Func<T5, T2>> place2,
            Expression<Func<T5, T3>> place3)
        {
            if (expression == null) return null;
            var parameter = place1.Parameters.Single();
            var hold1 = place1.Body;
            var hold2 = new ReplaceExpressionVisitor()
                .Attach(place2.Parameters[0], parameter)
                .Visit(place2.Body);
            var hold3 = new ReplaceExpressionVisitor()
                .Attach(place3.Parameters[0], parameter)
                .Visit(place3.Body);
            var newBody = new ReplaceExpressionVisitor()
                .Attach(expression.Parameters[0], hold1)
                .Attach(expression.Parameters[1], hold2)
                .Attach(expression.Parameters[2], hold3)
                .Visit(expression.Body);
            return Expression.Lambda<Func<T5, T4>>(newBody, parameter);
        }

        /// <summary>
        /// Combine some expressions with anonymous parameter type.
        /// </summary>
        /// <typeparam name="T1">The placehold 1.</typeparam>
        /// <typeparam name="T2">The placehold 2.</typeparam>
        /// <typeparam name="T3">The placehold 3.</typeparam>
        /// <typeparam name="T4">The placehold 4.</typeparam>
        /// <typeparam name="T5">The resulting placehold 5.</typeparam>
        /// <typeparam name="T6">The targetted placehold 6.</typeparam>
        /// <param name="expression">The expression to change.</param>
        /// <param name="objectTemplate">The source anonymous object template to use this function.</param>
        /// <param name="place1">The placehold 1 selector.</param>
        /// <param name="place2">The placehold 2 selector.</param>
        /// <param name="place3">The placehold 3 selector.</param>
        /// <param name="place4">The placehold 4 selector.</param>
        /// <returns>The compiled object.</returns>
        [return: CA.NotNullIfNotNull("expression")]
        public static Expression<Func<T6, T5>>? Combine<T1, T2, T3, T4, T5, T6>(
            this Expression<Func<T1, T2, T3, T4, T5>>? expression,
            T6 objectTemplate,
            Expression<Func<T6, T1>> place1,
            Expression<Func<T6, T2>> place2,
            Expression<Func<T6, T3>> place3,
            Expression<Func<T6, T4>> place4)
        {
            if (expression == null) return null;
            var parameter = place1.Parameters.Single();
            var hold1 = place1.Body;
            var hold2 = new ReplaceExpressionVisitor()
                .Attach(place2.Parameters[0], parameter)
                .Visit(place2.Body);
            var hold3 = new ReplaceExpressionVisitor()
                .Attach(place3.Parameters[0], parameter)
                .Visit(place3.Body);
            var hold4 = new ReplaceExpressionVisitor()
                .Attach(place4.Parameters[0], parameter)
                .Visit(place4.Body);
            var newBody = new ReplaceExpressionVisitor()
                .Attach(expression.Parameters[0], hold1)
                .Attach(expression.Parameters[1], hold2)
                .Attach(expression.Parameters[2], hold3)
                .Attach(expression.Parameters[3], hold4)
                .Visit(expression.Body);
            return Expression.Lambda<Func<T6, T5>>(newBody, parameter);
        }

        /// <summary>
        /// Combine some expressions with anonymous parameter type.
        /// </summary>
        /// <typeparam name="T1">The placehold 1.</typeparam>
        /// <typeparam name="T2">The placehold 2.</typeparam>
        /// <typeparam name="T3">The resulting placehold 3.</typeparam>
        /// <typeparam name="T4">The targetting placehold 4.</typeparam>
        /// <typeparam name="T5">The targetting placehold 5.</typeparam>
        /// <param name="expression">The expression to change.</param>
        /// <param name="objectTemplate1">The source anonymous object template to use this function.</param>
        /// <param name="objectTemplate2">The source anonymous object template to use this function.</param>
        /// <param name="place1">The placehold 1 selector.</param>
        /// <param name="place2">The placehold 2 selector.</param>
        /// <returns>The compiled object.</returns>
        [return: CA.NotNullIfNotNull("expression")]
        public static Expression<Func<T4, T5, T3>>? Combine<T1, T2, T3, T4, T5>(
            this Expression<Func<T1, T2, T3>>? expression,
            T4 objectTemplate1,
            T5 objectTemplate2,
            Expression<Func<T4, T5, T1>> place1,
            Expression<Func<T4, T5, T2>> place2)
        {
            if (expression == null) return null;
            var para1 = place1.Parameters[0];
            var para2 = place1.Parameters[1];
            var hold1 = place1.Body;
            var hold2 = new ReplaceExpressionVisitor()
                .Attach(place2.Parameters[0], para1)
                .Attach(place2.Parameters[1], para2)
                .Visit(place2.Body);
            var newBody = new ReplaceExpressionVisitor()
                .Attach(expression.Parameters[0], hold1)
                .Attach(expression.Parameters[1], hold2)
                .Visit(expression.Body);
            return Expression.Lambda<Func<T4, T5, T3>>(newBody, para1, para2);
        }

        /// <summary>
        /// Replace the parameter expression.
        /// </summary>
        /// <param name="self">The expression to change.</param>
        /// <param name="before">The expression to change from.</param>
        /// <param name="after">The expression to change to.</param>
        /// <returns>The result expression.</returns>
        public static Expression ReplaceWith(this Expression self, Expression before, Expression after)
        {
            return new ReplaceExpressionVisitor()
                .Attach(before, after)
                .Visit(self);
        }

        /// <summary>
        /// Combine the expression with <see cref="ExpressionType.OrElse"/>.
        /// </summary>
        /// <typeparam name="T">The source type.</typeparam>
        /// <param name="toBind">The list of predicates to combine.</param>
        /// <returns>The resulting expressions.</returns>
        public static Expression<Func<T, bool>> CombineOrElse<T>(this IReadOnlyList<Expression<Func<T, bool>>> toBind)
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

        /// <summary>
        /// Combine the expression with <see cref="ExpressionType.AndAlso"/>.
        /// </summary>
        /// <typeparam name="T">The source type.</typeparam>
        /// <param name="toBind">The list of predicates to combine.</param>
        /// <returns>The resulting expressions.</returns>
        public static Expression<Func<T, bool>> CombineAndAlso<T>(this IReadOnlyList<Expression<Func<T, bool>>> toBind)
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

        /// <summary>
        /// Combine some expression with other if condition satisfies.
        /// </summary>
        /// <typeparam name="T1">The computed source type.</typeparam>
        /// <typeparam name="T2">The computed source type.</typeparam>
        /// <param name="expression">The expression to use.</param>
        /// <param name="condition">The condition to satisfy.</param>
        /// <param name="other">The other expression to combine.</param>
        /// <returns>The combined expression.</returns>
        public static Expression<Func<T1, T2, bool>> CombineIf<T1, T2>(
            this Expression<Func<T1, T2, bool>> expression,
            bool condition,
            Expression<Func<T1, T2, bool>> other)
        {
            if (!condition) return expression;
            return expression.Combine(other);
        }

        /// <summary>
        /// Combine some expression with other if condition satisfies.
        /// </summary>
        /// <typeparam name="T">The computed source type.</typeparam>
        /// <param name="expression">The expression to use.</param>
        /// <param name="condition">The condition to satisfy.</param>
        /// <param name="other">The other expression to combine.</param>
        /// <returns>The combined expression.</returns>
        public static Expression<Func<T, bool>> CombineIf<T>(
            this Expression<Func<T, bool>> expression,
            bool condition,
            Expression<Func<T, bool>> other)
        {
            if (!condition) return expression;
            return expression.Combine(other);
        }
    }

    /// <summary>
    /// Short class for representing a lambda expression.
    /// </summary>
    public static class Expr
    {
        /// <summary>
        /// Of the expression.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <param name="expression">The core expression.</param>
        /// <returns>The same expression.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Expression<Func<T, bool>> Of<T>(Expression<Func<T, bool>> expression)
        {
            return expression;
        }

        /// <summary>
        /// Of the expression.
        /// </summary>
        /// <typeparam name="T1">The parameter type 1.</typeparam>
        /// <typeparam name="T2">The parameter type 2.</typeparam>
        /// <param name="expression">The core expression.</param>
        /// <returns>The same expression.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Expression<Func<T1, T2, bool>> Of<T1, T2>(Expression<Func<T1, T2, bool>> expression)
        {
            return expression;
        }
    }
}
