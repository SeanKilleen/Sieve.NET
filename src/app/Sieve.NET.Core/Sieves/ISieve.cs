namespace Sieve.NET.Core.Sieves
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    using Sieve.NET.Core.Options;

    public interface ISieve<TTypeOfObjectToFilter>
    {
        ISieve<TTypeOfObjectToFilter, TPropertyType> ForProperty<TPropertyType>(Expression<Func<TTypeOfObjectToFilter, TPropertyType>> propertyExpression);
    }

    public interface ISieve<TTypeOfObjectToFilter, TPropertyType>
    {
        InvalidValueBehavior InvalidValueBehavior { get; }
        EmptyValuesListBehavior EmptyValuesListBehavior { get; }
        IEnumerable<string> DefaultSeparators { get; }
        IEnumerable<string> Separators { get; }
        PropertyInfo PropertyToFilter { get; }
        Expression<Func<TTypeOfObjectToFilter, bool>> ToExpression();
        Func<TTypeOfObjectToFilter, bool> ToCompiledExpression();
        ICollection<TPropertyType> AcceptableValues { get; }

        ISieve<TTypeOfObjectToFilter, TPropertyType> ForProperty(Expression<Func<TTypeOfObjectToFilter, TPropertyType>> propertyExpression);

        ISieve<TTypeOfObjectToFilter, TPropertyType> ForValue(string stringValue);
        ISieve<TTypeOfObjectToFilter, TPropertyType> ForValue(TPropertyType acceptableValue);

        ISieve<TTypeOfObjectToFilter, TPropertyType> ForValues(IEnumerable<string> acceptableValues);
        ISieve<TTypeOfObjectToFilter, TPropertyType> ForValues(IEnumerable<TPropertyType> acceptableValues);
        ISieve<TTypeOfObjectToFilter, TPropertyType> ForValues(string valuesListToParse);

        ISieve<TTypeOfObjectToFilter, TPropertyType> ForAdditionalValue(string additionalValue);
        ISieve<TTypeOfObjectToFilter, TPropertyType> ForAdditionalValue(TPropertyType additionalValue);
        
        ISieve<TTypeOfObjectToFilter, TPropertyType> ForAdditionalValues(IEnumerable<string> acceptableValues);
        ISieve<TTypeOfObjectToFilter, TPropertyType> ForAdditionalValues(IEnumerable<TPropertyType> listOfValues);
        ISieve<TTypeOfObjectToFilter, TPropertyType> ForAdditionalValues(string listOfValues);
        
        ISieve<TTypeOfObjectToFilter, TPropertyType> WithSeparator(string newSeparatorString);
        ISieve<TTypeOfObjectToFilter, TPropertyType> WithSeparators(IEnumerable<string> separatorStrings);
        
        ISieve<TTypeOfObjectToFilter, TPropertyType> WithEmptyValuesListBehavior(EmptyValuesListBehavior emptyValuesListBehavior);
        ISieve<TTypeOfObjectToFilter, TPropertyType> WithInvalidValueBehavior(InvalidValueBehavior invalidValueBehavior);
    }
}