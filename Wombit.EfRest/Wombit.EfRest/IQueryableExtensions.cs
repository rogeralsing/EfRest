using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Wombit.EfRest
{
    public static class IQueryableExtensions
    {
        public static IQueryable<TOut> ExpandSelect<TIn, TOut>(this IQueryable<TIn> self, Expression<Func<TIn, TOut>> selector)
        {

            return self.Select(selector);
        }

        public static IEnumerable<TOut> ExpandSelect<TIn, TOut>(this IEnumerable<TIn> self, Expression<Func<TIn, TOut>> selector)
        {
            return null;
        }
    }
}
