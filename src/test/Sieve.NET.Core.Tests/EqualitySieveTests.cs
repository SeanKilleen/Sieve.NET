using System;
using System.Linq;

namespace Sieve.NET.Core.Tests
{
    using FluentAssertions;

    using Xunit;
    using Xunit.Extensions;

    public class EqualitySieveTests
    {
        [Fact]
        public void ForProperty_SetsPropertyName()
        {
            var propertyName = "AnInt";
            var sut = new EqualitySieve<ABusinessObject, int>()
                .ForProperty(propertyName);

            sut.PropertyName.Should().Be(propertyName);
        }

        [Fact]
        public void ForProperty_WithInvalidPropertyName_ThrowsException()
        {

            var propertyName = "PropertyNameThatDoesntExist";
            Action act = () => new EqualitySieve<ABusinessObject, int>()
                .ForProperty(propertyName);

            act.ShouldThrow<ArgumentException>()
                .And.Message.Should()
                .ContainEquivalentOf("property")
                .And.ContainEquivalentOf("does not exist")
                .And.ContainEquivalentOf(propertyName);
        }

        [Fact]
        public void ForProperty_DoesNotCareAboutCase()
        {
            var propertyName = "anInt"; //The property name is actually "AnInt"
            var sut = new EqualitySieve<ABusinessObject, int>()
                .ForProperty(propertyName);

            sut.PropertyName.Should().Be("AnInt");
            
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

            act.ShouldThrow<ArgumentNullException>()
                .And.Message.Should()
                .ContainEquivalentOf("property name")
                .And.ContainEquivalentOf("given")
                .And.ContainEquivalentOf("null or empty");
        }
            

        //TODO property type not matching yields an error.


    }

    public class EqualitySieve<TTypeOfObjectToFilter, TPropertyType>
    {
        public string PropertyName { get; private set; }

        public EqualitySieve<TTypeOfObjectToFilter, TPropertyType> ForProperty(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentNullException("the given property name is null or empty");
            }
            var matchingProperties = typeof(TTypeOfObjectToFilter).GetProperties().Where(x => x.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
            if (!matchingProperties.Any())
            {
                var exception = string.Format("Property '{0}' does not exist.", propertyName);
                throw new ArgumentException(exception);
            }
            
            PropertyName = matchingProperties.First().Name;
            return this;
        }

        //public EqualitySieve<TTypeOfObjectToFilter, TPropertyType> ForValue(TPropertyType propertyValue)
        //{
        //    throw new NotImplementedException();
        //}

        //public Expression<Func<TTypeOfObjectToFilter, bool>> ToExpression()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
