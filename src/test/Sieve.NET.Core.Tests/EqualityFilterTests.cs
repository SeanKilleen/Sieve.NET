using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieve.NET.Core.Tests
{
    using System.Linq.Expressions;

    using FluentAssertions;

    using Xunit;

    public class EqualityFilterTests
    {
        [Fact]
        public void GivenAnIntProperty_AndAnIntThatMatches_ReturnsTrueExpression()
        {
            var itemToTest = new ABusinessObject { AnInt = 3 };

            var filter = new Sieve<ABusinessObject>("AnInt", SieveType.EqualitySieve, 3);

            var sut = filter.GetCompiledExpression();

            sut.Invoke(itemToTest).Should().BeTrue();
        }

        [Fact]
        public void GivenAnIntProperty_AndAnIntThatDoesntMatch_ReturnsFalseExpression()
        {
            var itemToTest = new ABusinessObject { AnInt = 4 };

            var filter = new Sieve<ABusinessObject>("AnInt", SieveType.EqualitySieve, 3);

            var sut = filter.GetCompiledExpression();

            sut.Invoke(itemToTest).Should().BeFalse();
        }
    }

    public enum SieveType
    {
        EqualitySieve
    }

    public class Sieve<TTypeToFilter>
    {
        private string _propertyName;
        private SieveType _sieveType;

        private Type _objectToFilter;

        private IList<ConstantExpression> _acceptableValues = new List<ConstantExpression>();

        public Sieve(string propertyName, SieveType sieveType, int acceptableValue)
        {
            //TODO: Guard clauses

            _objectToFilter = typeof(TTypeToFilter);
            this._sieveType = sieveType;
            _propertyName = propertyName;

            _acceptableValues.Add(Expression.Constant(acceptableValue, typeof(int)));
        }

        public Expression<Func<TTypeToFilter, bool>> GetRawExpression()
        {

            ParameterExpression item = Expression.Parameter(typeof(TTypeToFilter), "item");
            MemberExpression property = Expression.PropertyOrField(item, _propertyName);

            var acceptableValues = new List<int?>();

            //take each expression constant and put it into a binary expression of property == constant expression
            //TODO: This uses First(). Remove that later to make compatible with lists.
            var binaryExpression = _acceptableValues.Select(constantExpressionItem => Expression.Equal(property, constantExpressionItem)).ToList().First();

            //for each binary expression, create a list of Expression lambdas 
            var lambda = Expression.Lambda<Func<TTypeToFilter, bool>>(binaryExpression, item);

            return lambda;
        }

        public Func<TTypeToFilter, bool> GetCompiledExpression()
        {
            var expression = this.GetRawExpression();
            return expression.Compile();
            
        }
            
    }
}
