using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Wombit.EfRest
{
    public class QueryProviderEx : IQueryProvider
    {
        private readonly IQueryProvider source;
        private string[] expand;
        private string[] fields;

        public QueryProviderEx(IQueryProvider source, string[] fields, string[] expand)
        {
            this.source = source;
            this.fields = fields;
            this.expand = expand;
        }

        #region IQueryProvider Members

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            Expression newExpression = new ExpressionReWriterVisitor(fields, expand).Visit(expression);
            IQueryable<TElement> query = source.CreateQuery<TElement>(newExpression);
            return new ObjectSetEx<TElement>(query, this.fields, this.expand);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            Expression newExpression = new ExpressionReWriterVisitor(fields, expand).Visit(expression);
            IQueryable query = source.CreateQuery(newExpression);
            return query;
        }

        public TResult Execute<TResult>(Expression expression)
        {
            Expression newExpression = new ExpressionReWriterVisitor(fields, expand).Visit(expression);
            return source.Execute<TResult>(newExpression);
        }

        public object Execute(Expression expression)
        {
            Expression newExpression = new ExpressionReWriterVisitor(fields, expand).Visit(expression);
            return source.Execute(newExpression);
        }

        #endregion
    }
}
