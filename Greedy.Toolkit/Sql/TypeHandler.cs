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
        private static ISet<Tuple<int, int, string>> insertSqlCache = new HashSet<Tuple<int, int, string>>();
        private static ISet<Tuple<int, int, string>> insertSqlWithIdentityCache = new HashSet<Tuple<int, int, string>>();
        private static ISet<Tuple<int, int, string>> updateSqlCache = new HashSet<Tuple<int, int, string>>();
        //private static QueryExpressionParser parser;

        public SqlGenerator SqlGenerator { get; private set; }

        internal TypeHandler(IDbConnection connection)
        {
            SqlGenerator = new SqlGenerator(connection);
            //parser = new QueryExpressionParser(connection);
        }

        public string GetInsertSql<T>(object obj)
        {
            var targetMapper = TypeMapperCache.GetTypeMapper(typeof(T));
            var sourceMapper = TypeMapperCache.GetTypeMapper(obj);
            if (insertSqlCache.Any(i => i.Item1 == sourceMapper.Code && i.Item2 == targetMapper.Code))
            {
                return insertSqlCache.Single(i => i.Item1 == sourceMapper.Code && i.Item2 == targetMapper.Code).Item3;
            }
            string sql = string.Empty;

            if (obj is T || obj is IEnumerable<T>)
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

        //public string GetSelectSql<T>(Expression expression, out IDictionary<string, dynamic> param)
        //{
        //    var expressionHandler = new ExpressionHandler(new ExpressionContext(this), new ExpressionHandleOption());
        //    var targetMapper = TypeMapperCache.GetTypeMapper(typeof(T));
        //    param = expressionHandler.Context.Parameters;
        //    return expressionHandler.GetSql(expression);
        //}

        public string GetFetchSql<T>(Expression<Func<T, bool>> expression, out IDictionary<string, dynamic> param)
        {
            var whereSql = string.Empty;
            param = null;
            if (expression != null)
            {
                var context = new ExpressionVisitorContext(SqlGenerator);
                var visitor = new WhereExpressionVisitor(context);
                visitor.Visit(expression);
                whereSql = visitor.Condition.ToSql(SqlGenerator);
                param = context.Parameters;
            }
            var targetMapper = TypeMapperCache.GetTypeMapper(typeof(T));
            return SqlGenerator.GetFetchSql(targetMapper, null, whereSql);
        }

        public string GetInsertSqlWithIdentity<T>(object obj)
        {
            var targetMapper = TypeMapperCache.GetTypeMapper(typeof(T));
            var sourceMapper = TypeMapperCache.GetTypeMapper(obj);
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
            var targetMapper = TypeMapperCache.GetTypeMapper(typeof(T));
            var sourceMapper = TypeMapperCache.GetTypeMapper(obj);
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

        public string GetUpdateSql<T>(Expression<Func<T, bool>> expression, IDictionary<Expression<Func<T, object>>, object> paramInput, out  IDictionary<string, dynamic> paramOuput)
        {
            var context = new ExpressionVisitorContext(SqlGenerator);
            var whereVisitor = new WhereExpressionVisitor(context);
            whereVisitor.Visit(expression);
            var whereSql = whereVisitor.Condition.ToSql(SqlGenerator);
            var setVisitor = new MemberExpressionVisitor(context);
            var setSql = new StringBuilder();
            foreach (KeyValuePair<Expression<Func<T, object>>, object> item in paramInput)
            {
                setVisitor.Visit(item.Key);
                setSql.AppendFormat("{0} = {1},", setVisitor.Column.ToSql(SqlGenerator), context.AddParameter(item.Value));
            }
            paramOuput = context.Parameters;
            return SqlGenerator.GetUpdateSql(TypeMapperCache.GetTypeMapper(typeof(T)), setSql.Remove(setSql.Length - 1, 1).ToString(), whereSql);
        }

        public string GetUpdateSql<T>(Expression<Func<T, bool>> expression, Expression<Func<T, object>> paramInput, object value, out  IDictionary<string, dynamic> paramOuput)
        {
            var context = new ExpressionVisitorContext(SqlGenerator);
            var whereVisitor = new WhereExpressionVisitor(context);
            whereVisitor.Visit(expression);
            var whereSql = whereVisitor.Condition.ToSql(SqlGenerator);

            var setVisitor = new MemberExpressionVisitor(context);
            setVisitor.Visit(paramInput);
            var setSql = string.Format("{0} = {1}", setVisitor.Column.ToSql(SqlGenerator), context.AddParameter(value));
            paramOuput = context.Parameters;
            return SqlGenerator.GetUpdateSql(TypeMapperCache.GetTypeMapper(typeof(T)), setSql, whereSql);
        }

        public string GetDeleteSql<T>(Expression<Func<T, bool>> expression, out IDictionary<string, dynamic> param)
        {
            var context = new ExpressionVisitorContext(SqlGenerator);
            var visitor = new WhereExpressionVisitor(context);
            visitor.Visit(expression);
            var whereSql = visitor.Condition.ToSql(SqlGenerator);
            param = context.Parameters;
            var targetMapper = TypeMapperCache.GetTypeMapper(typeof(T));
            return SqlGenerator.GetDeleteSql(targetMapper, null, whereSql);
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
    }
}
