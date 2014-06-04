namespace Sieve.NET.Core.Sieves
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Sieve.NET.Core.Exceptions;
    using Sieve.NET.Core.Options;

    /// <summary>
    /// The entry point to create a new Sieve. This is really a gateway to Sieve<BusinessObject, PropertyType>
    /// </summary>
    /// <typeparam name="TTypeOfObjectToFilter">The type of object you'd like to filter.</typeparam>
    /// <example>new EqualitySieve<MyBusinessObject>()</example>
    public class EqualitySieve<TTypeOfObjectToFilter> : ISieve<TTypeOfObjectToFilter>
    {
        /// <summary>
        /// This infers the property type and creates a Sieve specifically for that property type and name.
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
    /// An equality Sieve with the property type hard-coded as a second type parameter.
    /// This is the main class that performs the work of a Sieve.
    /// </summary>
    /// <typeparam name="TTypeOfObjectToFilter">The business object to filter.</typeparam>
    /// <typeparam name="TPropertyType">The type of the property you're filtering on (e.g. int)</typeparam>
    public class EqualitySieve<TTypeOfObjectToFilter, TPropertyType> : BaseSieve<TTypeOfObjectToFilter, TPropertyType>
    {




   


    }
}