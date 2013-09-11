using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Wombit.EfRest
{
    public class ObjectSetEx<T> : IOrderedQueryable<T>
    {
        private readonly QueryProviderEx provider;
        private readonly IQueryable<T> source;
        private string[] expand;
        private string[] fields;

        public ObjectSetEx(IQueryable<T> source, string[] fields, string[] expand)
        {
            this.source = source;
            this.fields = fields ?? new string[] { };
            this.expand = expand ?? new string[] { };
            provider = new QueryProviderEx(this.source.Provider, this.fields, this.expand);
        }

        #region IQueryableEx<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return source.GetEnumerator();
        }

        public Type ElementType
        {
            get { return source.ElementType; }
        }

        public Expression Expression
        {
            get { return source.Expression; }
        }

        public IQueryProvider Provider
        {
            get { return provider; }
        }
        #endregion
    }
}
