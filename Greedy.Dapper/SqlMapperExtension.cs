using Greedy.Toolkit.Expressions;
using Greedy.Toolkit.Sql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Dapper
{
    static partial class SqlMapper
    {
        private static TypeHandler typeHandler;


        private static TypeHandler GetTypeHandler(IDbConnection cnn)
        {
            if (typeHandler == null)
            {
                typeHandler = new TypeHandler(cnn);
            }
            return typeHandler;
        }

        public static int Insert<T>(this IDbConnection cnn, object param, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var type = param.GetType();
            if (type.IsArray && type != typeof(T))
            {
                object[] arr = (object[])param;
                var count = 0;
                foreach (var item in arr)
                {
                    count += cnn.Insert<T>(item);
                }
                return count;
            }
            else
                return cnn.Execute(GetTypeHandler(cnn).GetInsertSql<T>(param), param, transaction, commandTimeout);
        }

        public static long InsertWithIdentity<T>(this IDbConnection cnn, object param, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return cnn.ExecuteScalar<long>(GetTypeHandler(cnn).GetInsertSqlWithIdentity<T>(param), param, transaction, commandTimeout);
        }

        public static int Update<T>(this IDbConnection cnn, object param, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return cnn.Execute(GetTypeHandler(cnn).GetUpdateSql<T>(param), param, transaction, commandTimeout);
        }

        public static int Update<T>(this IDbConnection cnn, Expression<Func<T, bool>> expression, IDictionary<Expression<Func<T, object>>, object> param, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            IDictionary<string, dynamic> parameters;
            var sql = GetTypeHandler(cnn).GetUpdateSql<T>(expression, param, out parameters);
            return cnn.Execute(sql, parameters, transaction, commandTimeout);
        }

        public static int Update<T>(this IDbConnection cnn, Expression<Func<T, bool>> expression, Expression<Func<T, object>> param, object value, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            IDictionary<string, dynamic> parameters;
            var sql = GetTypeHandler(cnn).GetUpdateSql<T>(expression, param, value, out parameters);
            return cnn.Execute(sql, parameters, transaction, commandTimeout);
        }

        public static ColumnSet<T> Set<T>(this IDbConnection cnn)
        {
            return new ColumnSet<T>(cnn);
        }

        public static int Delete<T>(this IDbConnection cnn, Expression<Func<T, bool>> expression, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            IDictionary<string, dynamic> parameters;
            var sql = GetTypeHandler(cnn).GetDeleteSql<T>(expression, out parameters);
            return cnn.Execute(sql, parameters, transaction, commandTimeout);
        }

        public static IEnumerable<T> Get<T>(this IDbConnection cnn, Expression<Func<T, bool>> expression, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
        {
            IDictionary<string, dynamic> parameters;

            var sql = GetTypeHandler(cnn).GetFetchSql<T>(expression, out parameters);
            //cnn.Query<T>().Where(expression);
            return cnn.Query<T>(sql, parameters, transaction, buffered, commandTimeout);
        }

        public static IQueryable<T> Predicate<T>(this IDbConnection dbConnection)
        {
            return new DataQuery<T>(new QueryProvider(dbConnection));
        }
    }
}