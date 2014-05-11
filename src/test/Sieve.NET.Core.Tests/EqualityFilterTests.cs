using System.Text;
using System.Threading.Tasks;

namespace Sieve.NET.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    using FluentAssertions;

    using Xunit;

    // TODO: Given property type, if string is entered, convert it to that property type
    // TODO: Option to throw an error if an "acceptable value" isn't parseable
    // TODO: Multiple acceptable values as an OR statement
    // TODO: Default separator
    // TODO: Custom separator

    public class EqualityFilterTests
    {
        public class Integers
        {
            [Fact]
            public void GivenAnIntProperty_AndAnIntThatMatches_ReturnsTrueExpression()
            {
                var itemToTest = new ABusinessObject { AnInt = 3 };

                var filter = new Sieve<ABusinessObject, int>("AnInt", SieveType.EqualitySieve, 3);

                var sut = filter.GetCompiledExpression();

                sut.Invoke(itemToTest).Should().BeTrue();
            }

            [Fact]
            public void GivenAnIntProperty_AndAnIntThatDoesntMatch_ReturnsFalseExpression()
            {
                var itemToTest = new ABusinessObject { AnInt = 4 };

                var filter = new Sieve<ABusinessObject, int>("AnInt", SieveType.EqualitySieve, 3);

                var sut = filter.GetCompiledExpression();

                sut.Invoke(itemToTest).Should().BeFalse();
            }

            //[Fact]
            //public void GivenAnIntProperty_AndAnIntInStringFormThatMatches_ReturnsTrueExpression()
            //{
            //    var itemToTest = new ABusinessObject { AnInt = 3 };

            //    var filter = new Sieve<ABusinessObject, int>("AnInt", SieveType.EqualitySieve, "3");

            //    var sut = filter.GetCompiledExpression();

            //    sut.Invoke(itemToTest).Should().BeTrue();
            //}

            //[Fact]
            //public void GivenAnIntProperty_AndAnIntInStringFormThatDoesntMatch_ReturnsFalseExpression()
            //{
            //    var itemToTest = new ABusinessObject { AnInt = 4 };

            //    var filter = new Sieve<ABusinessObject, int>("AnInt", SieveType.EqualitySieve, "3");

            //    var sut = filter.GetCompiledExpression();

            //    sut.Invoke(itemToTest).Should().BeFalse();
            //}

            [Fact]
            public void GivenAnIntProperty_AndAListOfInts_MatchesOnlyIncludedInts()
            {
                var intOf1= new ABusinessObject { AnInt = 1 };
                var intOf2 = new ABusinessObject { AnInt = 2 };
                var intOf3 = new ABusinessObject { AnInt = 3 };

                var acceptableValues = new List<int> { 1, 3 };

                var filter = new Sieve<ABusinessObject, int>("AnInt", SieveType.EqualitySieve, acceptableValues);

                var sut = filter.GetCompiledExpression();
                
                sut.Invoke(intOf1).Should().BeTrue();
                sut.Invoke(intOf3).Should().BeTrue();
                sut.Invoke(intOf2).Should().BeFalse();



            }
            
        }


        // TODO: Dates
        // TODO: Longs
        // TODO: Strings w/case-sensitivity option & whitespace option

    }

    // TODO: Option on how to handle blank acceptable values (all items? throw error?)
    // TODO: "Contains" filter
    // TODO: LessThan Filter
    // TODO: GreaterThan Filter w/Inclusive option
    // TODO: LessThanInclusive Filter w/Inclusive option


}
