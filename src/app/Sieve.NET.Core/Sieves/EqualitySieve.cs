namespace Sieve.NET.Core.Sieves
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Sieve.NET.Core.Exceptions;

    /// <summary>
    ///     The entry point to create a new Sieve. This is really a gateway to Sieve<BusinessObject, PropertyType>
    /// </summary>
    /// <typeparam name="TTypeOfObjectToFilter">The type of object you'd like to filter.</typeparam>
    /// <example>new EqualitySieve<MyBusinessObject>()</example>
    public class EqualitySieve<TTypeOfObjectToFilter> : ISieve<TTypeOfObjectToFilter>
    {

        /// <summary>
        ///     This infers the property type and creates a Sieve specifically for that property type and name.
        /// </summary>
        /// <typeparam name="TPropertyType">The type of the property you're going to filter against.</typeparam>
        /// <param name="propertyExpression">An expression to help us get to a property.</param>
        /// <example>new EqualitySieve<ABusinessObject>().ForProperty(x=> x.AnInt)</example>
        /// <returns>A Sieve of <BusinessObject, PropertyType> (the latter is inferred from the expression.)</returns>
        public ISieve<TTypeOfObjectToFilter, TPropertyType> ForProperty<TPropertyType>(
            Expression<Func<TTypeOfObjectToFilter, TPropertyType>> propertyExpression)
        {
            return new EqualitySieve<TTypeOfObjectToFilter, TPropertyType>().ForProperty(propertyExpression);
        }

    }

    /// <summary>
    ///     An equality Sieve with the property type hard-coded as a second type parameter.
    ///     This is the main class that performs the work of a Sieve.
    /// </summary>
    /// <typeparam name="TTypeOfObjectToFilter">The business object to filter.</typeparam>
    /// <typeparam name="TPropertyType">The type of the property you're filtering on (e.g. int)</typeparam>
    public class EqualitySieve<TTypeOfObjectToFilter, TPropertyType> : BaseSieve<TTypeOfObjectToFilter, TPropertyType>
    {

        /// <returns>Returns the compiled version of the expression that the Sieve represents.</returns>
        /// <remarks>
        ///     Remember, a Func is essentially a compiled expression. Any library that needs
        ///     to look inside the expression itself will need to use the expression, and not the func.
        ///     However, if all you care about is the true/false within your own app, you can use this more easily.
        /// </remarks>
        public override Func<TTypeOfObjectToFilter, bool> ToCompiledExpression()
        {
            return this.ToExpression().Compile();
        }

        /// <summary>
        ///     This is the meat of what Sieve.NET can do. This method takes the sieve that has been defined and converts it to an
        ///     equality expression in .NET. This allows it to be passed to anything and creates an expression
        ///     that will evaluate to true if an object meets the Sieve's requirements.
        /// </summary>
        /// <returns>An expression of a Func of the object to filter and a boolean.</returns>
        /// <remarks>
        ///     This is where the power of Sieve.NET lies. You can pass this expression into something
        ///     like an OR/M that takes expressions and have it turn the expression into a SQL statement.
        /// </remarks>
        /// <exception cref="NoSieveValuesSuppliedException">
        ///     When the empty values list behavior is set to throw an exception and
        ///     no vaules have been supplied.
        /// </exception>
        /// <exception cref="SievePropertyNotSetException">When ForProperty() hasn't been called yet.</exception>
        public override Expression<Func<TTypeOfObjectToFilter, bool>> ToExpression()
        {
            if (this.PropertyToFilter == null)
            {
                throw new SievePropertyNotSetException(
                    "the PropertyToFilter of the Sieve object isn't set. Try calling ForProperty() to ensure it's set.");
            }

            ParameterExpression item = Expression.Parameter(typeof(TTypeOfObjectToFilter), "item");
            MemberExpression property = Expression.PropertyOrField(item, this.PropertyToFilter.Name);

            if (this.AcceptableValues == null || !this.AcceptableValues.Any())
            {
                return this.HandleEmptyAcceptableValuesList(item);
            }

            List<ConstantExpression> acceptableConstants =
                this.AcceptableValues.Select(
                    acceptableValueItem => Expression.Constant(acceptableValueItem, typeof(TPropertyType))).ToList();

            // take each expression constant and put it into a binary expression of property == constant expression
            List<BinaryExpression> binaryExpressions =
                acceptableConstants.Select(constantExpressionItem => Expression.Equal(property, constantExpressionItem))
                    .ToList();

            // for each binary expression, create a list of Expression lambdas 
            IEnumerable<Expression<Func<TTypeOfObjectToFilter, bool>>> lambdas =
                binaryExpressions.Select(
                    binExpression => Expression.Lambda<Func<TTypeOfObjectToFilter, bool>>(binExpression, item));

            Expression<Func<TTypeOfObjectToFilter, bool>> expressionToReturn =
                PredicateBuilder.False<TTypeOfObjectToFilter>();

            return lambdas.Aggregate(expressionToReturn, (current, lambdaItem) => current.Or(lambdaItem));
        }


    }
}