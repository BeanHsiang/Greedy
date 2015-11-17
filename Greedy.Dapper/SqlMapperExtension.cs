using Greedy.Toolkit.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
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

        public static int Insert<T>(this IDbConnection cnn, object param, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return cnn.Execute(GetTypeHandler(cnn).GetInsertSql<T>(param), param, transaction, commandTimeout, commandType);
        }

        public static long InsertWithIdentity<T>(this IDbConnection cnn, object param, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return cnn.ExecuteScalar<long>(GetTypeHandler(cnn).GetInsertSqlWithIdentity<T>(param), param, transaction, commandTimeout, commandType);
        }

        public static int Update<T>(this IDbConnection cnn, object param, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return cnn.Execute(GetTypeHandler(cnn).GetUpdateSql<T>(param), param, transaction, commandTimeout, commandType);
        }

        public static int Update<T>(this IDbConnection cnn, Expression<Func<T, bool>> expression, IDictionary<Expression<Func<T, object>>, object> param, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            IDictionary<string, dynamic> parameters;
            var sql = GetTypeHandler(cnn).GetUpdateSql<T>(expression, param, out parameters);
            return cnn.Execute(sql, parameters, transaction, commandTimeout, commandType);
        }

        public static int Update<T>(this IDbConnection cnn, Expression<Func<T, bool>> expression, Expression<Func<T, object>> param, object value, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            IDictionary<string, dynamic> parameters;
            var sql = GetTypeHandler(cnn).GetUpdateSql<T>(expression, param, value, out parameters);
            return cnn.Execute(sql, parameters, transaction, commandTimeout, commandType);
        }

        public static int Delete<T>(this IDbConnection cnn, Expression<Func<T, bool>> expression, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            IDictionary<string, dynamic> parameters;
            var sql = GetTypeHandler(cnn).GetDeleteSql<T>(expression, out parameters);
#if DEBUG
            System.Diagnostics.Debug.WriteLine(sql);
            return 0;
#else
            return cnn.Execute(sql, parameters, transaction, commandTimeout, commandType);
#endif
        }

        public static IEnumerable<T> Predicate<T>(this IDbConnection cnn, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return cnn.Query<T>(GetTypeHandler(cnn).GetInsertSql<T>(typeof(T)), null, transaction, buffered, commandTimeout, commandType);
        }
    }
}