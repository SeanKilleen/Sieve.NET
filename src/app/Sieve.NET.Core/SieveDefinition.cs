namespace Sieve.NET.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Creates a SieveDefinition -- a filter to apply to a given object.
    /// </summary>
    /// <typeparam name="TTypeToFilter">The type of the object you're filtering (e.g. typeof(MyBusinessObject)</typeparam>
    /// <typeparam name="TPropertyType">The type of the property you're filtering (e.g. typeof(int)</typeparam>
    public class SieveDefinition<TTypeToFilter, TPropertyType>
    {
        private string _propertyName;
        private SieveType _sieveType;

        private const string DEFAULT_SEPARATOR = "|";

        private IList<ConstantExpression> _acceptableValues = new List<ConstantExpression>();

        public SieveDefinition(string propertyName, SieveType sieveType, string acceptableValue)
        {
            this._sieveType = sieveType;
            this._propertyName = propertyName;

            if (typeof(TPropertyType) == typeof(int))
            {
                var theInt = int.Parse(acceptableValue);
                var constant = Expression.Constant(theInt, typeof(int));
                _acceptableValues.Add(constant);
            }
            
        }

        public SieveDefinition(string propertyName, SieveType sieveType, List<string> acceptableValues)
        {
            this._sieveType = sieveType;
            this._propertyName = propertyName;

            if (typeof(TPropertyType) == typeof(int))
            {
                foreach (var item in acceptableValues)
                {
                    var theInt = int.Parse(item);
                    var constant = Expression.Constant(theInt, typeof(int));
                    _acceptableValues.Add(constant);
                    
                }
            }

        }



        public SieveDefinition(string propertyName, SieveType sieveType, IEnumerable<TPropertyType> acceptableValues)
        {
            // TODO: Guard clauses

            this._sieveType = sieveType;
            this._propertyName = propertyName;

            foreach (var item in acceptableValues)
            {
                this._acceptableValues.Add(Expression.Constant(item, typeof(TPropertyType)));
            }

        }

        public SieveDefinition(string propertyName, SieveType sieveType, TPropertyType acceptableValue)
            : this(propertyName, sieveType, new List<TPropertyType> { acceptableValue })
        {
            
        }

       
        public Expression<Func<TTypeToFilter, bool>> GetRawExpression()
        {

            ParameterExpression item = Expression.Parameter(typeof(TTypeToFilter), "item");
            MemberExpression property = Expression.PropertyOrField(item, this._propertyName);

            // take each expression constant and put it into a binary expression of property == constant expression
            
            var binaryExpressions = this._acceptableValues.Select(constantExpressionItem => Expression.Equal(property, constantExpressionItem)).ToList();

            // for each binary expression, create a list of Expression lambdas 
            var lambdas = binaryExpressions.Select(binExpression => Expression.Lambda<Func<TTypeToFilter, bool>>(binExpression, item));

            var expressionToReturn = PredicateBuilder.False<TTypeToFilter>();

            foreach (var lambdaItem in lambdas)
            {
                expressionToReturn = expressionToReturn.Or(lambdaItem);
            }

            return expressionToReturn;
        }

        public Func<TTypeToFilter, bool> GetCompiledExpression()
        {
            var expression = this.GetRawExpression();
            return expression.Compile();
            
        }
            
    }
}