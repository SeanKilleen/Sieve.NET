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

    public class EqualitySieve<TTypeOfOjectToFilter>
    {
        public EqualitySieve<TTypeOfOjectToFilter, TPropertyType> ForProperty<TPropertyType>(
            Expression<Func<TTypeOfOjectToFilter, TPropertyType>> propertyExpression)
        {
            return new EqualitySieve<TTypeOfOjectToFilter, TPropertyType>().ForProperty(propertyExpression);
        }
    }

    public class EqualitySieve<TTypeOfObjectToFilter, TPropertyType>
    {
        public PropertyInfo PropertyToFilter { get; private set; }

        public ICollection<TPropertyType> AcceptableValues
        {
            get
            {
                return GetAcceptableValues();
            }
        }

        private List<TPropertyType> _knownAcceptableValues = new List<TPropertyType>();

        private ICollection<TPropertyType> GetAcceptableValues()
        {
            var separators = this.GetSeparatorsOrDefault();

            var parsedItems = this.ParseListOfPotentiallyAcceptableItems(separators);
            
            this.AddParsedItemsToPotentiallyAcceptableValues(parsedItems);

            var result = this.ParsePotentiallyAcceptableValues();

            return result;
        }

        private ICollection<TPropertyType> ParsePotentiallyAcceptableValues()
        {
            var result = new List<TPropertyType>();
            result.AddRange(_knownAcceptableValues);

            foreach (var stringItem in _potentiallyAcceptableValues)
            {
                try
                {
                    var convertedItem = Convert(stringItem.Trim());
                    result.Add(convertedItem);
                }
                catch
                {
                    if (this.InvalidValueBehavior != InvalidValueBehavior.ThrowInvalidSieveValueException)
                    {
                        continue;
                    }
                    var message = string.Format("Invalid value: {0}", stringItem);
                    throw new InvalidSieveValueException(message);
                }
            }

            return result;
        }

        private void AddParsedItemsToPotentiallyAcceptableValues(List<string> parsedItems)
        {
            parsedItems.ForEach(x => _potentiallyAcceptableValues.Add(x));
        }

        private List<string> ParseListOfPotentiallyAcceptableItems(IEnumerable<string> separators)
        {
            
            if (string.IsNullOrWhiteSpace(_potentiallyAcceptableValuesToParse)) { return new List<string>();}

            return _potentiallyAcceptableValuesToParse.Split(separators.ToArray(), StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();
        }

        private IEnumerable<string> GetSeparatorsOrDefault()
        {
            if (this.Separators == null || !this.Separators.Any())
            {
                this.Separators = this.DefaultSeparators;
            }

            // ReSharper disable once PossibleMultipleEnumeration -- this is covered by tests
            return Separators;
        }

        public IEnumerable<string> Separators { get; private set; }

        
        // ReSharper disable once MemberCanBePrivate.Global -- this is public so users can reference it.
        public EmptyValuesListBehavior EmptyValuesListBehavior { get; private set; }
        public InvalidValueBehavior InvalidValueBehavior { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global -- this is public on purpose so that folks can reference it if they need to.
        public readonly IEnumerable<string> DefaultSeparators = new List<string> {",", "|"};

        private List<string> _potentiallyAcceptableValues = new List<string>();
        private string _potentiallyAcceptableValuesToParse = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyLambda">A lambda that indicates the property that we'd like to filter on.</param>
        /// <returns></returns>
        /// <remarks>
        /// This is almost entirely possible due to the excellent answer on:
        /// http://stackoverflow.com/questions/671968/retrieving-property-name-from-lambda-expression
        /// </remarks>
        internal EqualitySieve<TTypeOfObjectToFilter, TPropertyType> ForProperty(Expression<Func<TTypeOfObjectToFilter, TPropertyType>> propertyLambda)
        {

            var propInfo = ExtractPropertyInfoFromLambda(propertyLambda);

            PropertyToFilter = propInfo;
            
            return this;
        }

        private PropertyInfo ExtractPropertyInfoFromLambda(Expression<Func<TTypeOfObjectToFilter, TPropertyType>> propertyLambda)
        {
            Type typePropertyShouldBeFrom = typeof(TTypeOfObjectToFilter);

            var member = propertyLambda.Body as MemberExpression;

            if (member == null)
            {
                throw new ArgumentException(string.Format("Expression '{0}' refers to a method, not a property.", propertyLambda));
            }

            var propInfo = member.Member as PropertyInfo;
            
            if (propInfo == null)
            {
                throw new ArgumentException(string.Format("Expression '{0}' refers to a field, not a property.", propertyLambda));
            }

            Debug.Assert(propInfo.ReflectedType != null, "propInfo.ReflectedType != null");
            
            if (typePropertyShouldBeFrom != propInfo.ReflectedType && !typePropertyShouldBeFrom.IsSubclassOf(propInfo.ReflectedType))
            {
                throw new ArgumentException(string.Format("Expression '{0}' refers to a property that is not from type {1}.", propertyLambda, typePropertyShouldBeFrom));
            }
            
            return propInfo;
        }

        public EqualitySieve<TTypeOfObjectToFilter, TPropertyType> ForValue(TPropertyType acceptableValue)
        {
            _knownAcceptableValues = new List<TPropertyType> {acceptableValue};
            return this;
        }

        public EqualitySieve<TTypeOfObjectToFilter, TPropertyType> ForValue(string stringValue)
        {
            _potentiallyAcceptableValues = new List<string>{stringValue};
            return this;
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
            if (this.EmptyValuesListBehavior == EmptyValuesListBehavior.LetNoObjectsThrough)
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

        private static TPropertyType Convert(string input)
        {
            var converter = TypeDescriptor.GetConverter(typeof(TPropertyType));
            return (TPropertyType)converter.ConvertFromString(input);
        }

        public Func<TTypeOfObjectToFilter, bool> ToCompiledExpression()
        {
            return this.ToExpression().Compile();
        }

        public EqualitySieve<TTypeOfObjectToFilter, TPropertyType> ForValues(IEnumerable<TPropertyType> acceptableValues)
        {
            List<TPropertyType> acceptableValuesList = acceptableValues.ToList();

            acceptableValuesList.ForEach(x=> _knownAcceptableValues.Add(x));

            return this;
        }
        public EqualitySieve<TTypeOfObjectToFilter, TPropertyType> ForValues(string valuesListToParse)
        {
            _potentiallyAcceptableValuesToParse = valuesListToParse;

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

        public EqualitySieve<TTypeOfObjectToFilter, TPropertyType> WithInvalidValueBehavior(InvalidValueBehavior invalidValueBehavior)
        {
            InvalidValueBehavior = invalidValueBehavior;

            return this;
        }
    }
}