using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Sql
{
    static class TypeMapperCache
    {
        private static IDictionary<IntPtr, ITypeMapper> typeMapperCache = new Dictionary<IntPtr, ITypeMapper>();

        internal static ITypeMapper GetTypeMapper(object obj)
        {
            var type = obj is Type ? (obj as Type) : obj.GetType();
            if (typeMapperCache.ContainsKey(type.TypeHandle.Value))
            {
                return typeMapperCache[type.TypeHandle.Value];
            }
            var mapper = new TypeMapper(obj);
            if (mapper.Name != TypeMapper.Dictionary_Name)
                typeMapperCache.Add(type.TypeHandle.Value, mapper);
            return mapper;
        }
    }
}
