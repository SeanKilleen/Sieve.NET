namespace Sieve.NET.Core.Sieves
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Sieve.NET.Core.Exceptions;
    using Sieve.NET.Core.Options;

    public class EqualitySieve<TTypeOfObjectToFilter, TPropertyType>
    {
        public PropertyInfo PropertyToFilter { get; private set; }
        public List<TPropertyType> AcceptableValues { get; private set; }
        public IEnumerable<string> Separators { get; private set; }
        public EmptyValuesListBehavior EmptyValuesListBehavior { get; private set; } 

        public readonly IEnumerable<string> DEFAULT_SEPARATORS = new List<string>{","};

        public EqualitySieve<TTypeOfObjectToFilter, TPropertyType> ForProperty(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new Exception("the given property name is null or empty");
            }

            var matchingProperty = FindMatchingProperty(propertyName);

            EnsurePropertyTypesMatch(matchingProperty);

            this.PropertyToFilter = matchingProperty;

            return this;
        }

        public EqualitySieve<TTypeOfObjectToFilter, TPropertyType> ForValue(TPropertyType acceptableValue)
        {
            this.AcceptableValues = new List<TPropertyType> { acceptableValue };
            return this;
        }

        public EqualitySieve<TTypeOfObjectToFilter, TPropertyType> ForValue(string stringValue)
        {
            try
            {
                TPropertyType convertedValue = Convert(stringValue);
                this.AcceptableValues = new List<TPropertyType> { convertedValue };
                return this;

            }
            catch (Exception)
            {
                this.AcceptableValues = new List<TPropertyType>();
                return this;
            }
        }

        public Expression<Func<TTypeOfObjectToFilter, bool>> ToExpression()
        {
            var item = Expression.Parameter(typeof(TTypeOfObjectToFilter), "item");
            var property = Expression.PropertyOrField(item, this.PropertyToFilter.Name);

            if (this.AcceptableValues == null || !this.AcceptableValues.Any())
            {
                return this.HandleEmptyAcceptableValuesList(item);
            }

            var acceptableConstants = this.AcceptableValues.Select(acceptableValueItem => Expression.Constant(acceptableValueItem, typeof(TPropertyType))).ToList();

            // take each expression constant and put it into a binary expression of property == constant expression
            var binaryExpressions = acceptableConstants.Select(constantExpressionItem => Expression.Equal(property, constantExpressionItem)).ToList();

            // for each binary expression, create a list of Expression lambdas 
            var lambdas = binaryExpressions.Select(binExpression => Expression.Lambda<Func<TTypeOfObjectToFilter, bool>>(binExpression, item));

            var expressionToReturn = PredicateBuilder.False<TTypeOfObjectToFilter>();

            return lambdas.Aggregate(expressionToReturn, (current, lambdaItem) => current.Or(lambdaItem));
        }

        private Expression<Func<TTypeOfObjectToFilter, bool>> HandleEmptyAcceptableValuesList(ParameterExpression parameter)
        {
            if (this.EmptyValuesListBehavior == EmptyValuesListBehavior.ThrowSieveValuesNotFoundException)
            {
                throw new NoSieveValuesSuppliedException();
            }
            var trueConstant = Expression.Constant(true, typeof(bool));
            var falseConstant = Expression.Constant(false, typeof(bool));

            if (this.EmptyValuesListBehavior == EmptyValuesListBehavior.LetAllObjectsThrough)
            {
                var binaryExpression = Expression.Equal(trueConstant, trueConstant);
                var expression = Expression.Lambda<Func<TTypeOfObjectToFilter, bool>>(binaryExpression, parameter);
                return expression;

            }
            if(this.EmptyValuesListBehavior == EmptyValuesListBehavior.LetNoObjectsThrough)
            {
                var binaryExpression = Expression.Equal(trueConstant, falseConstant);
                var expression = Expression.Lambda<Func<TTypeOfObjectToFilter, bool>>(binaryExpression, parameter);
                return expression;
            }

            throw new Exception("Could not determine empty values list behavior.");


        }

        public static implicit operator Expression<Func<TTypeOfObjectToFilter, bool>>(
            EqualitySieve<TTypeOfObjectToFilter, TPropertyType> sieve)
        {
            return sieve.ToExpression();
        }

        public static implicit operator Func<TTypeOfObjectToFilter, bool>(
            EqualitySieve<TTypeOfObjectToFilter, TPropertyType> sieve)
        {
            return sieve.ToCompiledExpression();
        }


        private static void EnsurePropertyTypesMatch(PropertyInfo matchingProperty)
        {
            if (matchingProperty.PropertyType == typeof(TPropertyType))
            {
                return;
            }
            var message =
                string.Format(
                    "property type doesn't match for property {0}. Sieve expects {1} but property is {2}",
                    matchingProperty.Name,
                    typeof(TPropertyType).Name,
                    matchingProperty.PropertyType);
            throw new ArgumentException(message);
        }

        private static PropertyInfo FindMatchingProperty(string propertyName)
        {
            var matchingProperties =
                typeof(TTypeOfObjectToFilter).GetProperties()
                    .Where(x => x.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));

            var propertyInfoList = matchingProperties as IList<PropertyInfo> ?? matchingProperties.ToList();

            if (propertyInfoList.Any())
            {
                return propertyInfoList.First();
            }

            var exception = string.Format("Property '{0}' does not exist.", propertyName);
            throw new PropertyNotFoundException(exception);
        }

        private static TPropertyType Convert(string input)
        {
            var converter = TypeDescriptor.GetConverter(typeof(TPropertyType));
            return (TPropertyType)converter.ConvertFromString(input);
        }

        //public Expression<Func<TTypeOfObjectToFilter, bool>> ToExpression()
        //{
        //    throw new NotImplementedException();
        //}

        public Func<TTypeOfObjectToFilter, bool> ToCompiledExpression()
        {
            return this.ToExpression().Compile();
        }

        public EqualitySieve<TTypeOfObjectToFilter,TPropertyType> ForValues(IEnumerable<TPropertyType> acceptableValues)
        {
            this.AcceptableValues = acceptableValues.ToList();
            return this;
        }
        public EqualitySieve<TTypeOfObjectToFilter, TPropertyType> ForValues(string valuesListToParse)
        {
            if (this.Separators == null || !this.Separators.Any())
            {
                this.Separators = this.DEFAULT_SEPARATORS;
            }
            var separators = this.Separators as string[] ?? this.Separators.ToArray();
            var arrayOfItems = valuesListToParse.Split(
                separators,
                StringSplitOptions.RemoveEmptyEntries).Where(x=>!string.IsNullOrWhiteSpace(x)).ToList();

            this.AcceptableValues = new List<TPropertyType>();
            foreach (var item in arrayOfItems)
            {
                this.AcceptableValues.Add(Convert(item.Trim()));
            }

            return this;
        }

        public EqualitySieve<TTypeOfObjectToFilter, TPropertyType> WithSeparator(string newSeparatorString)
        {
            if (!string.IsNullOrWhiteSpace(newSeparatorString))
            {
                this.Separators = new List<string> { newSeparatorString };
            }
            return this;
        }

        public EqualitySieve<TTypeOfObjectToFilter, TPropertyType> WithSeparators(List<string> separatorStrings)
        {
            if (separatorStrings != null && separatorStrings.Any())
            {
                this.Separators = separatorStrings;
            }
            return this;
        }

        public EqualitySieve<TTypeOfObjectToFilter, TPropertyType> WithEmptyValuesListBehavior(EmptyValuesListBehavior emptyValuesListBehavior)
        {
            this.EmptyValuesListBehavior = emptyValuesListBehavior;
            return this;
        }
    }
}