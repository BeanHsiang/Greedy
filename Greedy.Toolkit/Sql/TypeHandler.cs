using Greedy.Toolkit.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Greedy.Toolkit.Sql
{
    class TypeHandler
    {
        private static MemberMapperComparer MemberMapperComparer = new MemberMapperComparer();
        private static IDictionary<IntPtr, ITypeMapper> typeMapperCache = new Dictionary<IntPtr, ITypeMapper>();
        private static IList<Tuple<int, int, string>> insertSqlCache = new List<Tuple<int, int, string>>();
        private static IList<Tuple<int, int, string>> insertSqlWithIdentityCache = new List<Tuple<int, int, string>>();
        private static IList<Tuple<int, int, string>> updateSqlCache = new List<Tuple<int, int, string>>();

        public SqlGenerator SqlGenerator { get; private set; }

        internal TypeHandler(IDbConnection connection)
        {
            SqlGenerator = new SqlGenerator(connection);
        }

        public string GetInsertSql<T>(object obj)
        {
            var targetMapper = GetTypeMapper(typeof(T));
            var sourceMapper = GetTypeMapper(obj);
            if (insertSqlCache.Any(i => i.Item1 == sourceMapper.Code && i.Item2 == targetMapper.Code))
            {
                return insertSqlCache.Single(i => i.Item1 == sourceMapper.Code && i.Item2 == targetMapper.Code).Item3;
            }
            string sql = string.Empty;

            if (obj is T)
            {
                sql = SqlGenerator.GetInsertSql(targetMapper);
            }
            else
            {
                sql = SqlGenerator.GetInsertSql(RebuildMapper(sourceMapper, targetMapper));
            }
            insertSqlCache.Add(Tuple.Create(sourceMapper.Code, targetMapper.Code, sql));
            return sql;
        }

        public string GetInsertSqlWithIdentity<T>(object obj)
        {
            var targetMapper = GetTypeMapper(typeof(T));
            var sourceMapper = GetTypeMapper(obj);
            if (insertSqlWithIdentityCache.Any(i => i.Item1 == sourceMapper.Code && i.Item2 == targetMapper.Code))
            {
                return insertSqlWithIdentityCache.Single(i => i.Item1 == sourceMapper.Code && i.Item2 == targetMapper.Code).Item3;
            }
            string sql = string.Empty;

            if (obj is T)
            {
                sql = SqlGenerator.GetInsertSqlWithIdentity(targetMapper);
            }
            else
            {
                sql = SqlGenerator.GetInsertSqlWithIdentity(RebuildMapper(sourceMapper, targetMapper));
            }
            insertSqlWithIdentityCache.Add(Tuple.Create(sourceMapper.Code, targetMapper.Code, sql));
            return sql;
        }

        public string GetUpdateSql<T>(object obj)
        {
            var targetMapper = GetTypeMapper(typeof(T));
            var sourceMapper = GetTypeMapper(obj);
            if (updateSqlCache.Any(i => i.Item1 == sourceMapper.Code && i.Item2 == targetMapper.Code))
            {
                return updateSqlCache.Single(i => i.Item1 == sourceMapper.Code && i.Item2 == targetMapper.Code).Item3;
            }
            string sql = string.Empty;

            if (obj is T)
            {
                sql = SqlGenerator.GetUpdateSql(targetMapper);
            }
            else
            {
                sql = SqlGenerator.GetUpdateSql(RebuildMapper(sourceMapper, targetMapper));
            }
            updateSqlCache.Add(Tuple.Create(sourceMapper.Code, targetMapper.Code, sql));
            return sql;
        }

        public string GetDeleteSql<T>(Expression<Func<T, bool>> expression, out IDictionary<string, dynamic> param)
        {
            var expressionHandler = new ExpressionHandler(this, new ExpressionHandleOption());
            var targetMapper = GetTypeMapper(typeof(T));
            var whereSql = GetWhereSql(expression, expressionHandler, out param);
            if (expressionHandler.Option.UseAlias)
            {
                return SqlGenerator.GetDeleteSql(targetMapper, expressionHandler.Context.GetAlias(targetMapper), whereSql);
            }
            return SqlGenerator.GetDeleteSql(targetMapper, whereSql);
        }

        public string GetWhereSql<T>(Expression<Func<T, bool>> expression, ExpressionHandler expressionHandler, out IDictionary<string, dynamic> param)
        {
            var sql = expressionHandler.GetSql(expression);
            param = expressionHandler.Context.Parameters;
            return sql;
        }

        internal TypeMapper RebuildMapper(ITypeMapper source, ITypeMapper target)
        {
            var mapper = new TypeMapper()
            {
                Name = target.Name,
                TableName = target.Name,
                AllMembers = target.AllMembers.Intersect(source.AllMembers, MemberMapperComparer).ToList()
            };
            return mapper;
        }

        internal ITypeMapper GetTypeMapper(object obj)
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
