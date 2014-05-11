namespace Sieve.NET.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class Sieve<TTypeToFilter>
    {
        private string _propertyName;
        private SieveType _sieveType;

        private IList<ConstantExpression> _acceptableValues = new List<ConstantExpression>();

        public Sieve(string propertyName, SieveType sieveType, int acceptableValue)
        {
            // TODO: Guard clauses

            this._sieveType = sieveType;
            this._propertyName = propertyName;

            this._acceptableValues.Add(Expression.Constant(acceptableValue, typeof(int)));
        }

        public Sieve(string propertyName, SieveType sieveType, string acceptableValue) : this(propertyName, sieveType, int.Parse(acceptableValue))
        {
        }
        
        public Expression<Func<TTypeToFilter, bool>> GetRawExpression()
        {

            ParameterExpression item = Expression.Parameter(typeof(TTypeToFilter), "item");
            MemberExpression property = Expression.PropertyOrField(item, this._propertyName);

            // take each expression constant and put it into a binary expression of property == constant expression
            
            // TODO: This uses First(). Remove that later to make compatible with lists.
            var binaryExpression = this._acceptableValues.Select(constantExpressionItem => Expression.Equal(property, constantExpressionItem)).ToList().First();

            // for each binary expression, create a list of Expression lambdas 
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