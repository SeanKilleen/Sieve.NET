using System;
using System.Collections.Generic;
using System.Linq;

namespace Sieve.NET.Core.Sieves
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using Sieve.NET.Core.Exceptions;
    using Sieve.NET.Core.Options;

    public class BaseSieve<TTypeOfObjectToFilter, TPropertyType> : ISieve<TTypeOfObjectToFilter, TPropertyType>
    {
        /// <summary>
        /// The stored information about the property that the object will be filtered on.
        /// </summary>
        public PropertyInfo PropertyToFilter { get; private set; }

        /// <summary>
        /// Returns the list of acceptable values.
        /// This is generated automatically each time it is called, in case other
        /// methods have been called that would change the result.
        /// </summary>
        /// <exception cref="InvalidSieveValueException">When the invalid value behavior is set to throw an exception and an invalid value is found.</exception>
        public ICollection<TPropertyType> AcceptableValues
        {
            get
            {
                return GetAcceptableValues();
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global -- this is public on purpose so that folks can reference it if they need to.
        /// <summary>
        /// The list of separators that Sieve.NET will use by default if the user has not called WithSeparators().
        /// </summary>
        /// <remarks>This is public so that users can reference it and it's not a black box.</remarks>
        public IEnumerable<string> DefaultSeparators
        {
            get
            {
                return new List<string> { ",", "|" };
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global -- this is public so users can reference it.
        /// <summary>
        /// What the application should do if Sieve finds an empty values list when trying to generate an expression.
        /// </summary>
        /// <remarks>Defaults to "Allow All".</remarks>
        public EmptyValuesListBehavior EmptyValuesListBehavior { get; private set; }

        /// <summary>
        /// What the application should do if, when evaluating a value, it find that it is invalid.
        /// </summary>
        /// <remarks>Defaults to "Ignore the invalid value."</remarks>
        public InvalidValueBehavior InvalidValueBehavior { get; private set; }


        private IEnumerable<string> addedSeparators = new List<string>();
        private List<string> potentiallyAcceptableValues = new List<string>();
        private List<string> potentiallyAcceptableValuesToParse = new List<string>();
        private List<TPropertyType> knownAcceptableValues = new List<TPropertyType>();



        private ICollection<TPropertyType> GetAcceptableValues()
        {
            var separators = this.GetSeparatorsOrDefault();

            var parsedItems = this.ParseListOfPotentiallyAcceptableItems(separators);

            this.AddParsedItemsToPotentiallyAcceptableValues(parsedItems);

            var result = this.ParsePotentiallyAcceptableValues();

            return result;
        }

        private IEnumerable<string> GetSeparatorsOrDefault()
        {
            if (this.addedSeparators == null || !this.addedSeparators.Any())
            {
                return this.DefaultSeparators;
            }

            // ReSharper disable once PossibleMultipleEnumeration -- this is covered by tests
            return addedSeparators;
        }

        private List<string> ParseListOfPotentiallyAcceptableItems(IEnumerable<string> separators)
        {
            var result = new List<string>();
            foreach (var parseValueItem in this.potentiallyAcceptableValuesToParse)
            {
                // ReSharper disable once PossibleMultipleEnumeration -- covered by tests and works fine.
                result.AddRange(GetParsedValuesFromPotentiallyAcceptableParseString(parseValueItem, separators));
            }

            return result;
        }

        private IEnumerable<string> GetParsedValuesFromPotentiallyAcceptableParseString(string parseValueItem, IEnumerable<string> separators)
        {
            if (string.IsNullOrWhiteSpace(parseValueItem)) { return new List<string>(); }

            return parseValueItem.Split(separators.ToArray(), StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();
        }

        private void AddParsedItemsToPotentiallyAcceptableValues(List<string> parsedItems)
        {
            parsedItems.ForEach(x => this.potentiallyAcceptableValues.Add(x));
        }

        private static TPropertyType Convert(string input)
        {
            var converter = TypeDescriptor.GetConverter(typeof(TPropertyType));
            return (TPropertyType)converter.ConvertFromString(input);
        }

        private ICollection<TPropertyType> ParsePotentiallyAcceptableValues()
        {
            var result = new List<TPropertyType>();
            result.AddRange(this.knownAcceptableValues);

            foreach (var stringItem in this.potentiallyAcceptableValues)
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

            return result.Distinct().ToList();
        }

        /// <summary>
        /// The current list of separators in use.
        /// </summary>
        public IEnumerable<string> Separators
        {
            get
            {
                return this.GetSeparatorsOrDefault();
            }
        }






        /// <summary>
        /// Takes an item that is the same type as the property to filter against, and adds the item as an acceptable value.
        /// </summary>
        /// <param name="acceptableValue">A value of the same type of the property you're filtering on.</param>
        /// <returns>the current sieve with an updated list of acceptable values.</returns>
        /// <remarks>
        /// This clears any acceptable values that may have already existed.
        /// To add additional values, use ForAdditionalValue().
        /// </remarks>
        public ISieve<TTypeOfObjectToFilter, TPropertyType> ForValue(TPropertyType acceptableValue)
        {
            this.ClearPotentialValuesLists();
            this.knownAcceptableValues = new List<TPropertyType> { acceptableValue };
            return this;
        }

        /// <summary>
        /// Adds a value to the acceptable values list that is a string, but that can be converted to the property type.
        /// </summary>
        /// <param name="stringValue">
        /// A string that can be converted to the property type.
        ///  e.g. "1" for an int property type.) 
        /// </param>
        /// 
        /// <returns>The current sieve with an updated acceptable values list.</returns>
        /// <remarks>This clears any other acceptable values. You may want to use ForAdditionalValue() to add to the list.</remarks>
        /// <remarks>This assumes that your string can be converted to a property correctly. That responsibility is on the user.</remarks>
        public ISieve<TTypeOfObjectToFilter, TPropertyType> ForValue(string stringValue)
        {
            this.ClearPotentialValuesLists();
            this.potentiallyAcceptableValues = new List<string> { stringValue };
            return this;
        }

        /// <summary>
        /// This is the meat of what Sieve.NET can do. This method takes the sieve that has been defined and converts it to an 
        /// equality expression in .NET. This allows it to be passed to anything and creates an expression 
        /// that will evaluate to true if an object meets the Sieve's requirements.
        /// </summary>
        /// <returns>An expression of a Func of the object to filter and a boolean.</returns>
        /// <remarks>
        /// This is where the power of Sieve.NET lies. You can pass this expression into something
        /// like an OR/M that takes expressions and have it turn the expression into a SQL statement.</remarks>
        /// <exception cref="NoSieveValuesSuppliedException">When the empty values list behavior is set to throw an exception and no vaules have been supplied.</exception>
        /// <exception cref="SievePropertyNotSetException">When ForProperty() hasn't been called yet.</exception>
        public virtual Expression<Func<TTypeOfObjectToFilter, bool>> ToExpression()
        {
           throw new NotImplementedException("The BaseSieve type does not implement ToExpression()");
        }

        /// <returns>Returns the compiled version of the expression that the Sieve represents.</returns>
        /// <remarks>
        /// Remember, a Func is essentially a compiled expression. Any library that needs
        /// to look inside the expression itself will need to use the expression, and not the func.
        /// However, if all you care about is the true/false within your own app, you can use this more easily.
        /// </remarks>
        public virtual Func<TTypeOfObjectToFilter, bool> ToCompiledExpression()
        {
            throw new NotImplementedException("BaseSieve() does not implement ToCompiledExpression()");
        }

        /// <summary>
        /// Adds a list of items in the property type's format to the acceptable values list.
        /// </summary>
        /// <param name="acceptableValues">An enumerable of values that have the same type as the property we're filtering on.</param>
        /// <returns>An equality sieve with updated acceptable values.</returns>
        /// <remarks>This clears the previous acceptable values. To add to the list, use ForAdditionalValues().</remarks>
        public ISieve<TTypeOfObjectToFilter, TPropertyType> ForValues(IEnumerable<TPropertyType> acceptableValues)
        {
            ClearPotentialValuesLists();
            List<TPropertyType> acceptableValuesList = acceptableValues.ToList();

            acceptableValuesList.ForEach(x => this.knownAcceptableValues.Add(x));

            return this;
        }

        /// <summary>
        /// Adds a list of items in string format to the acceptable values list.
        /// </summary>
        /// <param name="acceptableValues">An enumerable of strings that can be converted to the property type we're filtering on.</param>
        /// <returns>An equality sieve with updated acceptable values.</returns>
        /// <remarks>This clears the previous acceptable values. To add to the list, use ForAdditionalValues().</remarks>
        /// <remarks>
        /// This assumes that any strings passed can be converted to the property type. That responsibility
        /// is on the user.
        /// </remarks>
        public ISieve<TTypeOfObjectToFilter, TPropertyType> ForValues(IEnumerable<string> acceptableValues)
        {
            this.ClearPotentialValuesLists();
            var acceptableValuesList = acceptableValues.ToList();

            this.potentiallyAcceptableValues.AddRange(acceptableValuesList.Where(x => !string.IsNullOrWhiteSpace(x)));

            return this;
        }

        /// <summary>
        /// Adds a list of items as one separated string to the acceptable values list.
        /// </summary>
        /// <param name="valuesListToParse">A string that can be turned into an enumerable of the property type.</param>
        /// <returns>An equality sieve with updated acceptable values.</returns>
        /// <remarks>This clears the previous acceptable values. To add to the list, use ForAdditionalValues().</remarks>
        /// <remarks> This assumes that any strings passed can be converted to the property type. That responsibility is on the user. </remarks>
        public ISieve<TTypeOfObjectToFilter, TPropertyType> ForValues(string valuesListToParse)
        {
            this.ClearPotentialValuesLists();
            this.potentiallyAcceptableValuesToParse.Add(valuesListToParse);

            return this;
        }

        /// <summary>
        /// Sets the separator that the Sieve will use to parse strings passed in with ForValues(string).
        /// </summary>
        /// <param name="newSeparatorString">The string to use as a separator.</param>
        /// <returns>An equality sieve set to use the given separator.</returns>
        public ISieve<TTypeOfObjectToFilter, TPropertyType> WithSeparator(string newSeparatorString)
        {
            if (!string.IsNullOrWhiteSpace(newSeparatorString))
            {
                this.addedSeparators = new List<string> { newSeparatorString };
            }
            return this;
        }

        /// <summary>
        /// Sets the separators that the Sieve will use to parse strings passed in with ForValues(string).
        /// </summary>
        /// <param name="separatorStrings">The strings to use as separators.</param>
        /// <returns>the current equality sieve set to use the given separators.</returns>
        /// <example>The sieve will use all specified separators. So, if "," and "|" are separated, "1,2|3" will become a list of 3 items -- 1, 2, and 3.</example>
        public ISieve<TTypeOfObjectToFilter, TPropertyType> WithSeparators(IEnumerable<string> separatorStrings)
        {
            if (separatorStrings != null && separatorStrings.Any())
            {
                this.addedSeparators = separatorStrings;
            }
            return this;
        }

        /// <summary>
        /// Sets the behavior that the application will take when it attempts to proess a Sieve with an empty acceptable values list.
        /// </summary>
        /// <param name="emptyValuesListBehavior">Describes the behavior (e.g. throw exception, let all objects through, etc.)</param>
        /// <returns>The current sieve, set to use the given Empty Values List behavior.</returns>
        public ISieve<TTypeOfObjectToFilter, TPropertyType> WithEmptyValuesListBehavior(EmptyValuesListBehavior emptyValuesListBehavior)
        {
            this.EmptyValuesListBehavior = emptyValuesListBehavior;
            return this;
        }

        /// <summary>
        /// Sets the behavior that the application will take when it attempts to process an invalid acceptable value.
        /// </summary>
        /// <param name="invalidValueBehavior">Describes the behavior (e.g. throw exception, ignore, etc.)</param>
        /// <returns>The current sieve, set to use the given Empty Values List behavior.</returns>
        public ISieve<TTypeOfObjectToFilter, TPropertyType> WithInvalidValueBehavior(InvalidValueBehavior invalidValueBehavior)
        {
            InvalidValueBehavior = invalidValueBehavior;

            return this;
        }

        /// <summary>
        /// Adds an additional value to the acceptable values list.
        /// </summary>
        /// <param name="additionalValue">An additional value of the same type as the property we're filtering on.</param>
        /// <returns>The current sieve with the additional acceptable value.</returns>
        public ISieve<TTypeOfObjectToFilter, TPropertyType> ForAdditionalValue(TPropertyType additionalValue)
        {
            this.knownAcceptableValues.Add(additionalValue);
            return this;
        }

        /// <summary>
        /// Adds an additional value to the acceptable values list.
        /// </summary>
        /// <param name="additionalValue">
        /// An additional value that  can be converted to 
        /// the tye of the property we're filtering on.</param>
        /// <returns>The current sieve with the additional acceptable value.</returns>
        public ISieve<TTypeOfObjectToFilter, TPropertyType> ForAdditionalValue(string additionalValue)
        {
            this.potentiallyAcceptableValues.Add(additionalValue);
            return this;
        }

        /// <summary>
        /// Adds a list of additional values to the acceptable values list.
        /// </summary>
        /// <param name="listOfValues"> A list of values of the same type as the property we're filtering on. </param>
        /// <returns>The current sieve with the additional acceptable values.</returns>
        public ISieve<TTypeOfObjectToFilter, TPropertyType> ForAdditionalValues(IEnumerable<TPropertyType> listOfValues)
        {
            this.knownAcceptableValues.AddRange(listOfValues);
            return this;
        }

        /// <summary>
        /// Adds a list of additional values to the acceptable values list.
        /// </summary>
        /// <param name="acceptableValues"> A list of values that can be converted to the same type as the property we're filtering on. </param>
        /// <returns>The current sieve with the additional acceptable values.</returns>
        /// <remarks>We expect the user to ensure that the values can actually be converted.</remarks>
        public ISieve<TTypeOfObjectToFilter, TPropertyType> ForAdditionalValues(IEnumerable<string> acceptableValues)
        {
            var acceptableValuesList = acceptableValues.ToList();

            this.potentiallyAcceptableValues.AddRange(acceptableValuesList.Where(x => !string.IsNullOrWhiteSpace(x)));

            return this;

        }

        /// <summary>
        /// Adds a string of separated additional values to the acceptable values list.
        /// </summary>
        /// <param name="listOfValues">
        /// A list of values in one separated string 
        /// that can be converted to the same type as the property we're filtering on. 
        /// </param>
        /// <returns>The current sieve with the additional acceptable values.</returns>
        /// <remarks>We expect the user to ensure that the values can actually be converted.</remarks>
        public ISieve<TTypeOfObjectToFilter, TPropertyType> ForAdditionalValues(string listOfValues)
        {
            this.potentiallyAcceptableValuesToParse.Add(listOfValues);
            return this;
        }









        /// <param name="propertyLambda">A lambda that indicates the property that we'd like to filter on.</param>
        /// <remarks>
        /// This is almost entirely possible due to the excellent answer on:
        /// http://stackoverflow.com/questions/671968/retrieving-property-name-from-lambda-expression
        /// </remarks>
        public ISieve<TTypeOfObjectToFilter, TPropertyType> ForProperty(Expression<Func<TTypeOfObjectToFilter, TPropertyType>> propertyLambda)
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

        internal Expression<Func<TTypeOfObjectToFilter, bool>> HandleEmptyAcceptableValuesList(ParameterExpression parameter)
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

        private void ClearPotentialValuesLists()
        {
            this.knownAcceptableValues = new List<TPropertyType>();
            this.potentiallyAcceptableValues = new List<string>();
            this.potentiallyAcceptableValuesToParse = new List<string>();
        }
    }
}
