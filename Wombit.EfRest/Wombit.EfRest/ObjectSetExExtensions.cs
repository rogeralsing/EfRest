using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombit.EfRest
{
    public static class ObjectSetExExtensions
    {
        public static ObjectSetEx<T> AsResource<T>(this IQueryable<T> self, string fields = null, string expand = null) where T : class
        {
            if (fields == "*")
                fields = "";

            var fieldArr = (fields ?? "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var expandArr = (expand ?? "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);


            expandArr = Permutate(expandArr);

            fieldArr = Permutate(fieldArr);



            return new ObjectSetEx<T>(self, fieldArr, expandArr);
        }

        private static string[] Permutate(string[] items)
        {
            var result = new List<string>(items);
            foreach (var item in items)
            {
                if (item.Contains("."))
                {
                    var parts = item.Split('.');
                    var current = parts[0];
                    for (int i = 1; i < parts.Length; i++)
                    {
                        current += "." + parts[i];
                        result.Add(current);
                    }
                }
            }
            return result.Distinct().ToArray();
        }
    }
}
