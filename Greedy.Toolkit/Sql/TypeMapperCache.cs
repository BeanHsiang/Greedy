using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Sql
{
    static class TypeMapperCache
    {
        private static IDictionary<IntPtr, ITypeMapper> typeMapperCache = new Dictionary<IntPtr, ITypeMapper>();
        private static IDictionary<IntPtr, Type> transferTypeMapperCache = new Dictionary<IntPtr, Type>();

        internal static ITypeMapper GetTypeMapper(object obj)
        {
            var type = obj is Type ? (obj as Type) : obj.GetType();
            lock (typeMapperCache)
            {
                var targetTypeHandle = type.TypeHandle.Value;
                if (transferTypeMapperCache.Keys.Contains(targetTypeHandle))
                {
                    targetTypeHandle = transferTypeMapperCache[targetTypeHandle].TypeHandle.Value;
                }

                if (typeMapperCache.ContainsKey(targetTypeHandle))
                {
                    return typeMapperCache[targetTypeHandle];
                }
                var mapper = new TypeMapper(obj);
                if (mapper.Name != TypeMapper.Dictionary_Name)
                    typeMapperCache.Add(type.TypeHandle.Value, mapper);
                return mapper;
            }
        }

        internal static void AddTransferTypeMapper(Type sourceType, Type targetType)
        {
            if (transferTypeMapperCache.Keys.Contains(sourceType.TypeHandle.Value))
            {
                return;
            }
            transferTypeMapperCache.Add(sourceType.TypeHandle.Value, targetType);
        }

        internal static Type GetTransferTypeMapper(Type sourceType)
        {
            if (transferTypeMapperCache.Keys.Contains(sourceType.TypeHandle.Value))
                return transferTypeMapperCache[sourceType.TypeHandle.Value];
            return sourceType;
        }
    }
}
