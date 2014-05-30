﻿
namespace Sieve.NET.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq.Expressions;

    using FluentAssertions;

    using Sieve.NET.Core.Exceptions;
    using Sieve.NET.Core.Options;
    using Sieve.NET.Core.Sieves;

    using Xunit;
    using Xunit.Extensions;

    public class EqualitySieveTests
    {
        private static readonly ABusinessObject ABusinessObjectWithAnIntOf1 = new ABusinessObject { AnInt = 1 };
        private static readonly ABusinessObject ABusinessObjectWithAnIntOf2 = new ABusinessObject { AnInt = 2 };
        private static readonly ABusinessObject ABusinessObjectWithAnIntOf3 = new ABusinessObject { AnInt = 3 };

        private static readonly ABusinessObject ABusinessObjectWithAStringOfOne = new ABusinessObject { AString = "One" };
        private static readonly ABusinessObject ABusinessObjectWithAStringOfTwo = new ABusinessObject { AString = "Two" };
        private static readonly ABusinessObject ABusinessObjectWithAStringOfThree = new ABusinessObject { AString = "Three" };

        private static readonly ABusinessObject ABusinessObjectFor2010725 = new ABusinessObject { ADateTime = new DateTime(2010, 7, 25) };
        private static readonly ABusinessObject ABusinessObjectFor2014526 = new ABusinessObject { ADateTime = new DateTime(2014, 5, 26) };
        private static readonly ABusinessObject ABusinessObjectForTodaysDate = new ABusinessObject { ADateTime = DateTime.Now };

        public class ToExpressionTests
        {

            [Fact]
            public void SingleValue_MatchesOnlyThatValue()
            {
                var sieve = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt).ForValue(1);

                var sut = sieve.ToExpression();

                sut.Compile().Invoke(ABusinessObjectWithAnIntOf1).Should().BeTrue();
                sut.Compile().Invoke(ABusinessObjectWithAnIntOf2).Should().BeFalse();
                sut.Compile().Invoke(ABusinessObjectWithAnIntOf3).Should().BeFalse();
            }
        }

        public class ToCompiledExpressionTests
        {
            [Fact]
            public void ToCompiledExpression_ReturnsCompiledExpression()
            {
                var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt).ForValue(1).ToCompiledExpression();

                sut.Invoke(ABusinessObjectWithAnIntOf1).Should().BeTrue();
                sut.Invoke(ABusinessObjectWithAnIntOf2).Should().BeFalse();
                sut.Invoke(ABusinessObjectWithAnIntOf3).Should().BeFalse();
            }
        }

        public class ImplicitExpressionConversionTests
        {
            [Fact]
            public void SingleValue_ImplicitConversionToFunc_WorksTheSameAsCompiledExpression()
            {
                //Implicit conversion means there's no call to ToCompiledExpression() necessary here.

                Func<ABusinessObject, bool> sut =
                    new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt).ForValue(1);

                sut.Invoke(ABusinessObjectWithAnIntOf1).Should().BeTrue();
                sut.Invoke(ABusinessObjectWithAnIntOf2).Should().BeFalse();
                sut.Invoke(ABusinessObjectWithAnIntOf3).Should().BeFalse();
            }

            [Fact]
            public void SingleValue_ImplicitConversionToExpression_WorksTheSameAsToExpression()
            {
                //Implicit conversion means there's no call to ToExpression() necessary here.

                Expression<Func<ABusinessObject, bool>> sut =
                    new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt).ForValue(1);

                sut.Compile().Invoke(ABusinessObjectWithAnIntOf1).Should().BeTrue();
                sut.Compile().Invoke(ABusinessObjectWithAnIntOf2).Should().BeFalse();
                sut.Compile().Invoke(ABusinessObjectWithAnIntOf3).Should().BeFalse();
            }
        }

        public class ForPropertyTests
        {
            public class UsingExpressionTests
            {
                [Fact]
                public void ForProperty_WithExpression_SetsProperty()
                {
                    const string PROPERTY_NAME = "AnInt";
                    var sut = new EqualitySieve<ABusinessObject>()
                        .ForProperty(x => x.AnInt);

                    sut.PropertyToFilter.Name.Should().Be(PROPERTY_NAME);

                }
            }
        }

        public class ForValueTests
        {

            [Fact]
            public void ForValue_DoesntDoAnythingAtFirst()
            {
                Action act = () => new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt)
                        .WithInvalidValueBehavior(InvalidValueBehavior.ThrowInvalidSieveValueException)
                        .ForValue("3abc");

                act.ShouldNotThrow<InvalidSieveValueException>(); // shouldn't throw because we don't care about it yet.
            }

            [Fact]
            public void ForValue_MattersWhenAcceptableValuesIsCalled()
            {
                EqualitySieve<ABusinessObject, int> sieve = null;

                Action actCreateSieve = () =>
                {
                    sieve =
                        new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt)
                        .ForValue("1abc")
                        .WithInvalidValueBehavior(InvalidValueBehavior.ThrowInvalidSieveValueException);
                };

                actCreateSieve.Invoke();

                Action actGetAcceptableValues = () =>
                {
                    Debug.Assert(sieve != null, "sieve != null");
                    // ReSharper disable once UnusedVariable -- used for the purposes of the test
                    var sut = sieve.AcceptableValues;
                };

                actCreateSieve.ShouldNotThrow<InvalidSieveValueException>();
                actGetAcceptableValues.ShouldThrow<InvalidSieveValueException>().And.Message.Should().ContainEquivalentOf("1abc");


            }

            [Fact]
            public void ForValue_MattersWhenToExpressionIsCalled()
            {
                EqualitySieve<ABusinessObject, int> sieve = null;

                Action actCreateSieve = () =>
                {
                    sieve =
                        new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt)
                        .ForValue("1abc")
                        .WithInvalidValueBehavior(InvalidValueBehavior.ThrowInvalidSieveValueException);
                };

                actCreateSieve.Invoke();

                Action actGetAcceptableValues = () =>
                {
                    Debug.Assert(sieve != null, "sieve != null");
                    // ReSharper disable once UnusedVariable -- used for purposes of this action only
                    var sut = sieve.ToExpression();
                };

                actCreateSieve.ShouldNotThrow<InvalidSieveValueException>();
                actGetAcceptableValues.ShouldThrow<InvalidSieveValueException>().And.Message.Should().ContainEquivalentOf("1abc");

            }

            public class ASingleStringTests
            {
                [Fact]
                public void CanBeCalledAgainToReplaceValueAfterFirstCausesAnError()
                {
                    const int ACCEPTABLE_VALUE = 1;
                    string acceptableValueInStringform = ACCEPTABLE_VALUE.ToString(CultureInfo.InvariantCulture);
                    
                    const string UNACCEPTABLE_VALUE = "1abc";
                    var expectedFinalList = new List<int> { ACCEPTABLE_VALUE };

                    var sut = new EqualitySieve<ABusinessObject>()
                        .ForProperty(x => x.AnInt)
                        .WithInvalidValueBehavior(InvalidValueBehavior.ThrowInvalidSieveValueException)
                        .ForValue(UNACCEPTABLE_VALUE);

                    // ReSharper disable once UnusedVariable -- used in deferred action
                    Action act = () => { var values = sut.AcceptableValues; };

                    act.ShouldThrow<InvalidSieveValueException>().And.Message.Should().ContainEquivalentOf(UNACCEPTABLE_VALUE);

                    sut.ForValue(acceptableValueInStringform);

                    sut.AcceptableValues.Should().BeEquivalentTo(expectedFinalList);
                }

                [Fact]
                public void WhenCalled_ReplacesPreviousValues()
                {
                    var expectedValuesFirstTime = new List<int> { 1 };
                    var expectedValuesSecondTime = new List<int> { 2 };

                    var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt)
                        .ForValue("1");

                    sut.AcceptableValues.Should().BeEquivalentTo(expectedValuesFirstTime);

                    sut.ForValue("2");

                    sut.AcceptableValues.Should().BeEquivalentTo(expectedValuesSecondTime);


                }

                [Fact]
                public void WithInvalidValue_IgnoredByDefault()
                {
                    var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt).ForValue("2abc");

                    sut.AcceptableValues.Should().BeEmpty();
                }

                [Fact]
                public void WithInvalidValue_AndInvalidValueBehaviorSetToThrowException_ExceptionIsThrownWhenAcceptableValuesIsCalled()
                {
                    var sieve =
                        new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt)
                            .WithInvalidValueBehavior(InvalidValueBehavior.ThrowInvalidSieveValueException)
                            .ForValue("2abc");

                    // ReSharper disable once UnusedVariable -- used for purposes of this action only
                    Action act = () => { var sut = sieve.AcceptableValues; };

                    act.ShouldThrow<InvalidSieveValueException>().And.Message.Should().ContainEquivalentOf("2abc");

                }

                [Fact]
                public void WithInvalidValue_AndInvalidValueBehaviorSetToThrowException_ExceptionIsThrownWhenToExpressionIsCalled()
                {
                    var sieve =
                        new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt)
                            .WithInvalidValueBehavior(InvalidValueBehavior.ThrowInvalidSieveValueException)
                            .ForValue("2abc");

                    // ReSharper disable once UnusedVariable -- used for purposes of this action only
                    Action act = () => { var sut = sieve.ToExpression(); };

                    act.ShouldThrow<InvalidSieveValueException>().And.Message.Should().ContainEquivalentOf("2abc");

                }

                [Fact]
                public void SingleString_WhenPropertyIsString_AddsToList()
                {
                    const string STRING_TO_TEST = "Hello World";

                    var sut = new EqualitySieve<ABusinessObject>()
                        .ForProperty(x => x.AString)
                        .ForValue(STRING_TO_TEST);

                    var expectedList = new List<string> { STRING_TO_TEST };
                    sut.AcceptableValues.Should().BeEquivalentTo(expectedList);
                }

                [Fact]
                public void SingleString_WhenPropertyIsNotAString_ConvertsAndAddsToList()
                {
                    const string STRING_TO_TEST = "123";

                    var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt).ForValue(STRING_TO_TEST);

                    var expectedList = new List<int> { 123 };
                    sut.AcceptableValues.Should().BeEquivalentTo(expectedList);

                }

                [Fact]
                public void SingleString_WhenInvalidToConvert_SetsAcceptableValuesToEmptyList()
                {
                    const string STRING_TO_TEST = "123abc";

                    var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt).ForValue(STRING_TO_TEST);

                    var expectedList = new List<int>();
                    sut.AcceptableValues.Should().BeEquivalentTo(expectedList);
                }
            }

            public class ItemOfPropertyTypeTests
            {
                [Fact]
                public void WhenCalled_ReplacesPreviousItems()
                {
                    var expectedValuesFirstTime = new List<int> { 1 };
                    var expectedValuesSecondTime = new List<int> { 2 };

                    var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt)
                        .ForValue(1);

                    sut.AcceptableValues.Should().BeEquivalentTo(expectedValuesFirstTime);

                    sut.ForValue(2);

                    sut.AcceptableValues.Should().BeEquivalentTo(expectedValuesSecondTime);

                }
                [Fact]
                public void SingleItemOfPropertyType_SetsAcceptableValuesList()
                {
                    const int NUMBER_TO_TEST = 123;

                    var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt).ForValue(NUMBER_TO_TEST);

                    var expectedList = new List<int> { NUMBER_TO_TEST };
                    sut.AcceptableValues.Should().BeEquivalentTo(expectedList);
                }
            }
        }

        public class ForAdditionalValueTests
        {
            public class TPropertyTypeTests
            {
                [Fact]
                public void WhenNoValueExists_AddsValue()
                {
                    var expectedValues = new List<int> { 1 };
                    var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt)
                        .ForAdditionalValue(1);

                    sut.AcceptableValues.Should().BeEquivalentTo(expectedValues);
                }

                [Fact]
                public void WhenValueExists_AddsAdditionalValue()
                {
                    var expectedValues = new List<int> { 1, 2 };
                    var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt)
                        .ForValue(1).ForAdditionalValue(2);

                    sut.AcceptableValues.Should().BeEquivalentTo(expectedValues);
                }
            }

            public class Strings
            {
                [Fact]
                public void WhenNoValueExists_AddsValue()
                {
                    var expectedValues = new List<int> { 1, 2 };
                    var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt)
                        .ForValue("1").ForAdditionalValue("2");

                    sut.AcceptableValues.Should().BeEquivalentTo(expectedValues);

                }

                [Fact]
                public void WhenValueExists_AddsAdditionalValue()
                {
                    var expectedValues = new List<int> { 1 };
                    var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt)
                        .ForAdditionalValue("1");

                    sut.AcceptableValues.Should().BeEquivalentTo(expectedValues);
                }
            }


        }

        public class ForAdditionalValuesTests
        {
                public class TPropertyTypeTests
            {
                [Fact]
                public void WhenNoValuesExist_AddsValues()
                {
                    throw new NotImplementedException();
                }

                [Fact]
                public void WhenValuesExist_AddsAdditionalValues()
                {
                    throw new NotImplementedException();
                }
            }

            public class Strings
            {
                [Fact]
                public void WhenNoValueListsExist_AddsValues()
                {
                    throw new NotImplementedException();
                }

                [Fact]
                public void WhenValueExists_AddsAdditionalValue()
                {
                    throw new NotImplementedException();

                }
            }
        }


        public class ForValuesTests
        {
            public class EnumerableOfPropertyTypeTests
            {
                [Fact]
                public void ListOfInts_BecomesListOfAcceptableValues()
                {
                    var valuesToTry = new List<int> { 1, 3 };

                    var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt).ForValues(valuesToTry);
                    sut.AcceptableValues.Should().BeEquivalentTo(valuesToTry);

                    var compiled = sut.ToCompiledExpression();
                    compiled.Invoke(ABusinessObjectWithAnIntOf1).Should().BeTrue();
                    compiled.Invoke(ABusinessObjectWithAnIntOf2).Should().BeFalse();
                    compiled.Invoke(ABusinessObjectWithAnIntOf3).Should().BeTrue();
                }

                [Fact]
                public void ArrayOfInts_BecomesListOfAcceptableValues()
                {
                    var valuesToTry = new[] { 1, 3 };

                    var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt).ForValues(valuesToTry);
                    sut.AcceptableValues.Should().BeEquivalentTo(valuesToTry);
                }

                [Fact]
                public void ListOfStrings_BecomesListOfAcceptableStringValues()
                {
                    var valuesToTry = new[] { "One", "Three" };

                    var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AString).ForValues(valuesToTry);
                    sut.AcceptableValues.Should().BeEquivalentTo(valuesToTry);

                    var compiled = sut.ToCompiledExpression();

                    compiled.Invoke(ABusinessObjectWithAStringOfOne).Should().BeTrue();
                    compiled.Invoke(ABusinessObjectWithAStringOfTwo).Should().BeFalse();
                    compiled.Invoke(ABusinessObjectWithAStringOfThree).Should().BeTrue();

                }
            }

            public class SeparatedStringTests
            {
                [Fact]
                public void WithEntryContainingInvalidValue_IgnoredByDefault()
                {
                    var expectedValues = new List<int> { 1 };

                    var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt).ForValues("1, 2abc");
                    sut.AcceptableValues.ShouldBeEquivalentTo(expectedValues);
                }

                [Fact]
                public void WithEntryContainingInvalidValue_AndInvalidValueBehaviorSetToThrowException_ExceptionIsThrownWhenCallingAcceptableValues()
                {

                    var sieve = new EqualitySieve<ABusinessObject>()
                        .ForProperty(x => x.AnInt)
                        .WithInvalidValueBehavior(InvalidValueBehavior.ThrowInvalidSieveValueException)
                        .ForValues("1, 2abc");

                    // ReSharper disable once UnusedVariable -- used for purposes of this action only
                    Action act = () => { var sut = sieve.AcceptableValues; };

                    act.ShouldThrow<InvalidSieveValueException>().And.Message.Should().ContainEquivalentOf("2abc");

                }

                [Fact]
                public void WithEntryContainingInvalidValue_AndInvalidValueBehaviorSetToThrowException_ExceptionIsThrownWhenCallingToExpression()
                {

                    var sieve = new EqualitySieve<ABusinessObject>()
                        .ForProperty(x => x.AnInt)
                        .ForValues("1, 2abc")
                        .WithInvalidValueBehavior(InvalidValueBehavior.ThrowInvalidSieveValueException);

                    // ReSharper disable once UnusedVariable -- used for purposes of this action only
                    Action act = () => { var sut = sieve.ToExpression(); };

                    act.ShouldThrow<InvalidSieveValueException>().And.Message.Should().ContainEquivalentOf("2abc");

                }


                [Fact]
                public void MultipleStringEntries_ForStringBasedSieve_BecomeMultipleAcceptableValues()
                {
                    var expectedValues = new List<string> { "One", "Three" };

                    var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AString).ForValues("One, Three");

                    sut.AcceptableValues.ShouldBeEquivalentTo(expectedValues);

                    var compiled = sut.ToCompiledExpression();

                    compiled.Invoke(ABusinessObjectWithAStringOfOne).Should().BeTrue();
                    compiled.Invoke(ABusinessObjectWithAStringOfTwo).Should().BeFalse();
                    compiled.Invoke(ABusinessObjectWithAStringOfThree).Should().BeTrue();
                }

                [Fact]
                public void MultipleCommaSeparatedIntsInStringForm_BecomeAcceptableInts()
                {
                    var valuesToTry = new List<int> { 1, 3 };

                    var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt).ForValues("1, 3");
                    sut.AcceptableValues.Should().BeEquivalentTo(valuesToTry);

                    var compiled = sut.ToCompiledExpression();
                    compiled.Invoke(ABusinessObjectWithAnIntOf1).Should().BeTrue();
                    compiled.Invoke(ABusinessObjectWithAnIntOf2).Should().BeFalse();
                    compiled.Invoke(ABusinessObjectWithAnIntOf3).Should().BeTrue();
                }

                [Fact]
                public void MultipleCommaSeparatedDatesInStringForm_BecomeAcceptableDates()
                {
                    var valuesToTry = new List<DateTime> { new DateTime(2010, 7, 25), new DateTime(2014, 5, 26) };

                    var sut = new EqualitySieve<ABusinessObject>()
                        .ForProperty(x => x.ADateTime)
                        .ForValues("7/25/2010, 5/26/2014");

                    sut.AcceptableValues.Should().BeEquivalentTo(valuesToTry);

                    var compiled = sut.ToCompiledExpression();

                    compiled.Invoke(ABusinessObjectFor2010725).Should().BeTrue();
                    compiled.Invoke(ABusinessObjectFor2014526).Should().BeTrue();
                    compiled.Invoke(ABusinessObjectForTodaysDate).Should().BeFalse();
                }

                [Fact]
                public void EmptyStringEntries_AreIgnored()
                {
                    var valuesToTry = new List<int> { 1, 3 };

                    var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt).ForValues("1, , , , , 3");
                    sut.AcceptableValues.Should().BeEquivalentTo(valuesToTry);

                    var compiled = sut.ToCompiledExpression();
                    compiled.Invoke(ABusinessObjectWithAnIntOf1).Should().BeTrue();
                    compiled.Invoke(ABusinessObjectWithAnIntOf2).Should().BeFalse();
                    compiled.Invoke(ABusinessObjectWithAnIntOf3).Should().BeTrue();
                }

                [Fact]
                public void SpacesAreIgnoreOnEitherSideOfitems()
                {
                    var valuesToTry = new List<int> { 1, 3 };

                    var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt).ForValues(" 1 ,  3 ");
                    sut.AcceptableValues.Should().BeEquivalentTo(valuesToTry);

                    var compiled = sut.ToCompiledExpression();
                    compiled.Invoke(ABusinessObjectWithAnIntOf1).Should().BeTrue();
                    compiled.Invoke(ABusinessObjectWithAnIntOf2).Should().BeFalse();
                    compiled.Invoke(ABusinessObjectWithAnIntOf3).Should().BeTrue();
                }
            }
        }

        public class WithSeparatorTests
        {
            [Fact]
            public void GivenSeparator_ChangesSeparator()
            {
                var separatorString = new List<string> { "|" };
                var sut = new EqualitySieve<ABusinessObject>()
                    .ForProperty(x => x.AnInt)
                    .WithSeparator("|");
                sut.Separators.ShouldBeEquivalentTo(separatorString);
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData("    ")]
            public void GivenEmptySeparator_DoesntChangeSeparator(string separatorToTry)
            {
                var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt);

                var currentSeparator = sut.Separators;

                sut = sut.WithSeparator(separatorToTry);

                sut.Separators.ShouldBeEquivalentTo(currentSeparator);
            }

            [Fact]
            public void WhenChangingSeparator_AcceptableValuesUseNewSeparator()
            {
                var expectedList = new List<int> { 1, 2, 3 };

                var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt).WithSeparator("|").ForValues("1 |2  | 3");

                sut.AcceptableValues.ShouldBeEquivalentTo(expectedList);
            }
        }

        public class WithSeparatorsTests
        {
            [Fact]
            public void GivenSeparators_ChangesSeparators()
            {
                var separatorString = new List<string> { "|", "," };
                var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt)
                    .WithSeparators(separatorString);
                sut.Separators.ShouldBeEquivalentTo(separatorString);
            }

            [Fact]
            public void GivenNullList_DoesntChangeSeparator()
            {
                var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt);

                var currentSeparators = sut.Separators;

                sut = sut.WithSeparators(null);

                sut.Separators.ShouldBeEquivalentTo(currentSeparators);
            }

            [Fact]
            public void GivenEmptyList_DoesntChangeSeparator()
            {
                var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt);

                var currentSeparators = sut.Separators;

                sut = sut.WithSeparators(new List<string>());

                sut.Separators.ShouldBeEquivalentTo(currentSeparators);
            }

            [Fact]
            public void WhenChangingSeparators_AcceptableValuesUseAllNewSeparators()
            {
                var expectedList = new List<int> { 1, 2, 3 };

                var separators = new List<string> { "|", "," };
                var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt)
                    .WithSeparators(separators).ForValues("1 ,2  | 3");

                sut.AcceptableValues.ShouldBeEquivalentTo(expectedList);
            }

        }

        public class WithEmptyValuesBehaviorTests
        {
            [Fact]
            public void DefaultBehaviorIsToLetAllThrough()
            {
                //no values defined
                var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt).ToCompiledExpression();

                sut.Invoke(ABusinessObjectWithAnIntOf1).Should().BeTrue();
                sut.Invoke(ABusinessObjectWithAnIntOf2).Should().BeTrue();
                sut.Invoke(ABusinessObjectWithAnIntOf3).Should().BeTrue();
            }

            [Fact]
            public void WithLetNoneThroughOption_DoesntAllowAnyThrough()
            {
                //no values defined
                var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt)
                    .WithEmptyValuesListBehavior(EmptyValuesListBehavior.LetNoObjectsThrough).ToCompiledExpression();

                sut.Invoke(ABusinessObjectWithAnIntOf1).Should().BeFalse();
                sut.Invoke(ABusinessObjectWithAnIntOf2).Should().BeFalse();
                sut.Invoke(ABusinessObjectWithAnIntOf3).Should().BeFalse();
            }

            [Fact]
            public void WithThrowExceptionOption_ThrowsANoSieveValuesSuppliedException()
            {
                //no values defined
                Action act = () => new EqualitySieve<ABusinessObject>().ForProperty(x => x.AnInt)
                    .WithEmptyValuesListBehavior(EmptyValuesListBehavior.ThrowSieveValuesNotFoundException)
                    .ToCompiledExpression();

                act.ShouldThrow<NoSieveValuesSuppliedException>();
            }

        }

        public class WithInvalidValueBehaviorTests
        {

            [Fact]
            public void WithoutCalling_InvalidValueBehaviorIsSetByDefaultToIgnore()
            {
                var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.ADateTime);

                sut.InvalidValueBehavior.ShouldBeEquivalentTo(InvalidValueBehavior.IgnoreInvalidValue);

            }

            [Fact]
            public void WithoutCalling_InvalidValuesAreIgnoredByDefault()
            {
                const string STRING_VALUES = "7/25/2010, 12/1abc/2012";
                var expectedList = new List<DateTime> { new DateTime(2010, 7, 25) };
                var sut = new EqualitySieve<ABusinessObject>().ForProperty(x => x.ADateTime).ForValues(STRING_VALUES);

                sut.AcceptableValues.ShouldBeEquivalentTo(expectedList);
            }

            [Fact]
            public void WhenSpecifyingItShouldbeIgnored_InvalidValuesAreIgnoredByDefault()
            {
                const string STRING_VALUES = "7/25/2010, 12/1abc/2012";

                var expectedList = new List<DateTime> { new DateTime(2010, 7, 25) };

                var sut =
                    new EqualitySieve<ABusinessObject>().ForProperty(x => x.ADateTime)
                        .WithInvalidValueBehavior(InvalidValueBehavior.IgnoreInvalidValue)
                        .ForValues(STRING_VALUES);

                sut.InvalidValueBehavior.ShouldBeEquivalentTo(InvalidValueBehavior.IgnoreInvalidValue);
                sut.AcceptableValues.ShouldBeEquivalentTo(expectedList);
            }

            [Fact]
            public void WhenSpecifyingAnExceptionBeThrown_InvalidValueThrowsInvalidSieveValueExceptionWhenCallingAcceptableValues()
            {
                const string STRING_VALUES = "7/25/2010, 12/1abc/2012";

                var sieve = new EqualitySieve<ABusinessObject>().ForProperty(x => x.ADateTime)
                    .WithInvalidValueBehavior(InvalidValueBehavior.ThrowInvalidSieveValueException)
                    .ForValue(STRING_VALUES);

                // ReSharper disable once UnusedVariable -- used for purposes of this action only
                Action act = () => { var sut = sieve.AcceptableValues; };

                act.ShouldThrow<InvalidSieveValueException>().And.Message.Should().ContainEquivalentOf("12/1abc/2012");

            }

            [Fact]
            public void WhenSpecifyingAnExceptionBeThrown_InvalidValueThrowsInvalidSieveValueExceptionWhenCallingToExpression()
            {
                const string STRING_VALUES = "7/25/2010, 12/1abc/2012";

                var sieve = new EqualitySieve<ABusinessObject>().ForProperty(x => x.ADateTime)
                    .WithInvalidValueBehavior(InvalidValueBehavior.ThrowInvalidSieveValueException)
                    .ForValue(STRING_VALUES);

                // ReSharper disable once UnusedVariable -- used for purposes of this action only
                Action act = () => { var sut = sieve.ToExpression(); };

                act.ShouldThrow<InvalidSieveValueException>().And.Message.Should().ContainEquivalentOf("12/1abc/2012");
            }
        }
    }
}
