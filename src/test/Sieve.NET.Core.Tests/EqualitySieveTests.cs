
namespace Sieve.NET.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;

    using FluentAssertions;
    using Xunit;
    using Xunit.Extensions;

    public class EqualitySieveTests
    {
        public class ToExpressionTests
        {
            [Fact]
            public void SingleValue_MatchesOnlyThatValue()
            {
                var aBusinessObjectWithAnIntOf1 = new ABusinessObject { AnInt = 1 };
                var aBusinessObjectWithAnIntOf2 = new ABusinessObject { AnInt = 2 };
                var aBusinessObjectWithAnIntOf3 = new ABusinessObject { AnInt = 3 };

                var sieve = new EqualitySieve<ABusinessObject, int>().ForProperty("AnInt").ForValue(1);

                var sut = sieve.ToExpression();

                sut.Compile().Invoke(aBusinessObjectWithAnIntOf1).Should().BeTrue();
                sut.Compile().Invoke(aBusinessObjectWithAnIntOf2).Should().BeFalse();
                sut.Compile().Invoke(aBusinessObjectWithAnIntOf3).Should().BeFalse();
            }
        }

        public class ToCompiledExpressionTests
        {
            [Fact]
            public void ToCompiledExpression_ReturnsCompiledExpression()
            {
                var aBusinessObjectWithAnIntOf1 = new ABusinessObject { AnInt = 1 };
                var aBusinessObjectWithAnIntOf2 = new ABusinessObject { AnInt = 2 };
                var aBusinessObjectWithAnIntOf3 = new ABusinessObject { AnInt = 3 };

                var sut = new EqualitySieve<ABusinessObject, int>().ForProperty("AnInt").ForValue(1).ToCompiledExpression();

                sut.Invoke(aBusinessObjectWithAnIntOf1).Should().BeTrue();
                sut.Invoke(aBusinessObjectWithAnIntOf2).Should().BeFalse();
                sut.Invoke(aBusinessObjectWithAnIntOf3).Should().BeFalse();

            }
        }

        public class ImplicitExpressionConversionTests
        {
            [Fact]
            public void SingleValue_ImplicitConversionToFunc_WorksTheSameAsCompiledExpression()
            {
                var aBusinessObjectWithAnIntOf1 = new ABusinessObject { AnInt = 1 };
                var aBusinessObjectWithAnIntOf2 = new ABusinessObject { AnInt = 2 };
                var aBusinessObjectWithAnIntOf3 = new ABusinessObject { AnInt = 3 };

                //Implicit conversion means there's no call to ToCompiledExpression() necessary here.

                Func<ABusinessObject, bool> sut =
                    new EqualitySieve<ABusinessObject, int>().ForProperty("AnInt").ForValue(1);

                sut.Invoke(aBusinessObjectWithAnIntOf1).Should().BeTrue();
                sut.Invoke(aBusinessObjectWithAnIntOf2).Should().BeFalse();
                sut.Invoke(aBusinessObjectWithAnIntOf3).Should().BeFalse();

            }

            [Fact]
            public void SingleValue_ImplicitConversionToExpression_WorksTheSameAsToExpression()
            {
                var aBusinessObjectWithAnIntOf1 = new ABusinessObject { AnInt = 1 };
                var aBusinessObjectWithAnIntOf2 = new ABusinessObject { AnInt = 2 };
                var aBusinessObjectWithAnIntOf3 = new ABusinessObject { AnInt = 3 };

                //Implicit conversion means there's no call to ToExpression() necessary here.

                Expression<Func<ABusinessObject, bool>> sut =
                    new EqualitySieve<ABusinessObject, int>().ForProperty("AnInt").ForValue(1);

                sut.Compile().Invoke(aBusinessObjectWithAnIntOf1).Should().BeTrue();
                sut.Compile().Invoke(aBusinessObjectWithAnIntOf2).Should().BeFalse();
                sut.Compile().Invoke(aBusinessObjectWithAnIntOf3).Should().BeFalse();
            }
        }
       
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

                act.ShouldThrow<PropertyNotFoundException>()
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

        public class ForValuesTests
        {

            public class IEnumerableOfPropertyTypeTests
            {
                [Fact]
                public void ListOfInts_BecomesListOfAcceptableValues()
                {
                    var valuesToTry = new List<int> { 1, 3 };

                    var aBusinessObjectWithAnIntOf1 = new ABusinessObject { AnInt = 1 };
                    var aBusinessObjectWithAnIntOf2 = new ABusinessObject { AnInt = 2 };
                    var aBusinessObjectWithAnIntOf3 = new ABusinessObject { AnInt = 3 };


                    var sut = new EqualitySieve<ABusinessObject, int>().ForProperty("AnInt").ForValues(valuesToTry);
                    sut.AcceptableValues.Should().BeEquivalentTo(valuesToTry);

                    var compiled = sut.ToCompiledExpression();
                    compiled.Invoke(aBusinessObjectWithAnIntOf1).Should().BeTrue();
                    compiled.Invoke(aBusinessObjectWithAnIntOf2).Should().BeFalse();
                    compiled.Invoke(aBusinessObjectWithAnIntOf3).Should().BeTrue();
                }

                [Fact]
                public void ArrayOfInts_BecomesListOfAcceptableValues()
                {
                    var valuesToTry = new[] { 1, 3 };

                    var sut = new EqualitySieve<ABusinessObject, int>().ForProperty("AnInt").ForValues(valuesToTry);
                    sut.AcceptableValues.Should().BeEquivalentTo(valuesToTry);
                }

                [Fact]
                public void ListOfStrings_BecomesListOfAcceptableStringValues()
                {
                    var aBusinessObjectWithAStringOfOne = new ABusinessObject { AString = "One"};
                    var aBusinessObjectWithAStringOfTwo = new ABusinessObject { AString = "Two" };
                    var aBusinessObjectWithAStringOfThree = new ABusinessObject { AString = "Three" };

                    var valuesToTry = new[] { "One", "Three" };

                    var sut = new EqualitySieve<ABusinessObject, string>().ForProperty("AString").ForValues(valuesToTry);
                    sut.AcceptableValues.Should().BeEquivalentTo(valuesToTry);

                    var compiled = sut.ToCompiledExpression();

                    compiled.Invoke(aBusinessObjectWithAStringOfOne).Should().BeTrue();
                    compiled.Invoke(aBusinessObjectWithAStringOfTwo).Should().BeFalse();
                    compiled.Invoke(aBusinessObjectWithAStringOfThree).Should().BeTrue();

                }
            }

            public class SeparatedStringTests
            {
                //TODO: empty string entries are ignored
                //TODO: spaces are ignored on either side
                //TODO: multiple comma separated string ints become list of acceptable values
                //TODO: multiple comma separate strings become list of strings with string type
            }

        }

    }
}
