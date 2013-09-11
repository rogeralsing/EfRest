using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wombit.EfRest
{
    static class TypeGenerator
    {
        private static ConcurrentDictionary<string, Type> lookup = new ConcurrentDictionary<string, Type>();
        public static Type CreateType(IList<Tuple<string, Type>> template)
        {
            var keys = template.Select(i => string.Format("{0}|{1}|", i.Item1, i.Item2.Name)).ToArray();
            var key = string.Join("", keys);
            if (lookup.ContainsKey(key))
                return lookup[key];

            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = "tmpAssembly";
            AssemblyBuilder assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder module = assemblyBuilder.DefineDynamicModule("tmpModule");

            // create a new type builder
            TypeBuilder typeBuilder = module.DefineType("dummy", TypeAttributes.Public | TypeAttributes.Class);

            foreach (var tuple in template)
            {
                FieldBuilder field = typeBuilder.DefineField(tuple.Item1, tuple.Item2, FieldAttributes.Public);
            }

            // Generate our type
            Type generetedType = typeBuilder.CreateType();
            lookup.TryAdd(key, generetedType);

            return generetedType;
        }
    }
}
