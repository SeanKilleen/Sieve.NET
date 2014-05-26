using System;
using System.Linq;

namespace Sieve.NET.Core.Tests
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using FluentAssertions;
    using Xunit;
    using Xunit.Extensions;

    public class EqualitySieveTests
    {
        public class ForPropertyTests
        {
            [Fact]
            public void ForProperty_SetsProperty()
            {
                const string PROPERTY_NAME = "AnInt";
                var sut = new EqualitySieve<ABusinessObject, int>()
                    .ForProperty(PROPERTY_NAME);

                sut.PropertyToFilter.Name.Should().Be(PROPERTY_NAME);
            }

            [Fact]
            public void ForProperty_WithInvalidPropertyName_ThrowsException()
            {

                // ReSharper disable once StringLiteralTypo
                const string PROPERTY_NAME = "PropertyNameThatDoesntExist";
                Action act = () => new EqualitySieve<ABusinessObject, int>()
                    .ForProperty(PROPERTY_NAME);

                act.ShouldThrow<ArgumentException>()
                    .And.Message.Should()
                    .ContainEquivalentOf("property")
                    .And.ContainEquivalentOf("does not exist")
                    .And.ContainEquivalentOf(PROPERTY_NAME);
            }

            [Fact]
            public void ForProperty_DoesNotCareAboutCase()
            {
                const string PROPERTY_NAME = "anInt"; //The property name is actually "AnInt"
                var sut = new EqualitySieve<ABusinessObject, int>()
                    .ForProperty(PROPERTY_NAME);

                sut.PropertyToFilter.Name.Should().Be("AnInt");
                sut.PropertyToFilter.PropertyType.Should().Be<int>();

            }

            [Theory]
            [InlineData("")]
            [InlineData(null)]
            [InlineData("    ")]
            public void ForProperty_WithEmptyPropertyName_ThrowsException(string itemToTry)
            {
                string propertyName = itemToTry;

                Action act = () => new EqualitySieve<ABusinessObject, int>()
                    .ForProperty(propertyName);

                act.ShouldThrow<Exception>()
                    .And.Message.Should()
                    .ContainEquivalentOf("property name")
                    .And.ContainEquivalentOf("given")
                    .And.ContainEquivalentOf("null or empty");
            }

            [Fact]
            // ReSharper disable once IdentifierTypo
            public void ForProperty_WhenGivenTypeDoesntMatchActualPropertyType_ThrowsError()
            {
                const string PROPERTY_NAME = "ADateTime";

                //A DateTime is not an int, so this should throw an error.
                Action act = () => new EqualitySieve<ABusinessObject, int>()
                    .ForProperty(PROPERTY_NAME);

                act.ShouldThrow<ArgumentException>()
                    .And.Message.Should()
                    .ContainEquivalentOf("property")
                    .And.ContainEquivalentOf("DateTime")
                    .And.ContainEquivalentOf("int")
                    .And.ContainEquivalentOf("doesn't match")
                    .And.ContainEquivalentOf("type")
                    .And.ContainEquivalentOf(PROPERTY_NAME);
            }

       
        }

        public class ForValueTests
        {
            public class ASingleStringTests
            {
                [Fact]
                public void SingleString_WhenPropertyIsString_AddsToList()
                {
                    const string STRING_TO_TEST = "Hello World";

                    var sut = new EqualitySieve<ABusinessObject, string>()
                        .ForProperty("AString")
                        .ForValue(STRING_TO_TEST);

                    var expectedList = new List<string> { STRING_TO_TEST };
                    sut.AcceptableValues.Should().BeEquivalentTo(expectedList);
                }

                [Fact]
                public void SingleString_WhenPropertyIsNotAString_ConvertsAndAddsToList()
                {
                    const string STRING_TO_TEST = "123";

                    var sut = new EqualitySieve<ABusinessObject, int>().ForProperty("AnInt").ForValue(STRING_TO_TEST);

                    var expectedList = new List<int> { 123 };
                    sut.AcceptableValues.Should().BeEquivalentTo(expectedList);
                    
                }

                [Fact]
                public void SingleString_WhenInvalidToConvert_SetsAcceptableValuesToEmptyList()
                {
                    const string STRING_TO_TEST = "123abc";

                    var sut = new EqualitySieve<ABusinessObject, int>().ForProperty("AnInt").ForValue(STRING_TO_TEST);

                    var expectedList = new List<int>();
                    sut.AcceptableValues.Should().BeEquivalentTo(expectedList);
                    
                }
            }

            public class ItemOfPropertyTypeTests
            {
                [Fact]
                public void SingleItemOfPropertyType_SetsAcceptableValuesList()
                {
                    const int NUMBER_TO_TEST = 123;

                    var sut = new EqualitySieve<ABusinessObject, int>().ForProperty("AnInt").ForValue(NUMBER_TO_TEST);

                    var expectedList = new List<int> { NUMBER_TO_TEST };
                    sut.AcceptableValues.Should().BeEquivalentTo(expectedList);
                }
            }
        }

        public class ForValues
        {
            public class SeparatedString
            {
                //TODO: Finish
            }

            public class ListOfPropertyType
            {
                //TODO: Finish
            }
        }
     
    }

    public class EqualitySieve<TTypeOfObjectToFilter, TPropertyType>
    {
        public PropertyInfo PropertyToFilter { get; private set; }
        public List<TPropertyType> AcceptableValues { get; private set; }

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
            AcceptableValues = new List<TPropertyType> {acceptableValue};
            return this;
        }

        public EqualitySieve<TTypeOfObjectToFilter, TPropertyType> ForValue(string stringValue)
        {
            try
            {
                TPropertyType convertedValue = Convert(stringValue);
                AcceptableValues = new List<TPropertyType> { convertedValue };
                return this;

            }
            catch (Exception)
            {
                AcceptableValues = new List<TPropertyType>();
                return this;
            }
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
            throw new ArgumentException(exception);
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


    }
}
