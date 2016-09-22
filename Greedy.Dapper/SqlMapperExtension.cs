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
        private static IDbTransaction transaction;
        private static IDictionary<int, int> deepCount = new Dictionary<int, int>();

        private static TypeHandler GetTypeHandler(IDbConnection cnn)
        {
            if (typeHandler == null)
            {
                typeHandler = new TypeHandler(cnn);
            }
            return typeHandler;
        }

        /// <summary>
        /// Insert one or more objects as per T, that are the same type 
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public static int Insert<T>(this IDbConnection cnn, object param, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var type = param.GetType();
            if (type.IsArray && !(param is IEnumerable<T>))
            {
                object[] arr = (object[])param;
                var count = 0;
                foreach (var item in arr)
                {
                    count += cnn.Insert<T>(item, transaction, commandTimeout);
                }
                return count;
            }
            else
                return cnn.Execute(GetTypeHandler(cnn).GetInsertSql<T>(param), param, transaction, commandTimeout);
        }

        /// <summary>
        /// Insert an object as per T
        /// </summary>
        /// <returns>THe identity value of last inserted row</returns>
        public static long InsertWithIdentity<T>(this IDbConnection cnn, object param, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return cnn.ExecuteScalar<long>(GetTypeHandler(cnn).GetInsertSqlWithIdentity<T>(param), param, transaction, commandTimeout);
        }

        /// <summary>
        /// Update one or more objects as per T, that are the same type 
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public static int Update<T>(this IDbConnection cnn, object param, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return cnn.Execute(GetTypeHandler(cnn).GetUpdateSql<T>(param), param, transaction, commandTimeout);
        }

        /// <summary>
        /// Update rows according to some special condition expressions
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public static int Update<T>(this IDbConnection cnn, Expression<Func<T, bool>> expression, IDictionary<Expression<Func<T, object>>, object> param, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            IDictionary<string, dynamic> parameters;
            var sql = GetTypeHandler(cnn).GetUpdateSql<T>(expression, param, out parameters);
            return cnn.Execute(sql, parameters, transaction, commandTimeout);
        }

        /// <summary>
        /// Update rows according to a special condition expression
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public static int Update<T>(this IDbConnection cnn, Expression<Func<T, bool>> expression, Expression<Func<T, object>> param, object value, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            IDictionary<string, dynamic> parameters;
            var sql = GetTypeHandler(cnn).GetUpdateSql<T>(expression, param, value, out parameters);
            return cnn.Execute(sql, parameters, transaction, commandTimeout);
        }

        /// <summary>
        /// Get a update-prepared set
        /// </summary>
        /// <returns>ColumnSet type</returns>
        public static ColumnSet<T> Set<T>(this IDbConnection cnn)
        {
            return new ColumnSet<T>(cnn);
        }

        /// <summary>
        /// Delete rows according to a special condition expression
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public static int Delete<T>(this IDbConnection cnn, Expression<Func<T, bool>> expression, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            IDictionary<string, dynamic> parameters;
            var sql = GetTypeHandler(cnn).GetDeleteSql<T>(expression, out parameters);
            return cnn.Execute(sql, parameters, transaction, commandTimeout);
        }

        /// <summary>
        /// Execute a query, returning the data typed as per T
        /// </summary>
        /// <returns>A sequence of data of the supplied type</returns>
        public static IEnumerable<T> Get<T>(this IDbConnection cnn, Expression<Func<T, bool>> expression, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
        {
            IDictionary<string, dynamic> parameters;

            var sql = GetTypeHandler(cnn).GetFetchSql<T>(expression, out parameters);
            //cnn.Query<T>().Where(expression);
            return cnn.Query<T>(sql, parameters, transaction, buffered, commandTimeout);
        }

        /// <summary>
        /// Get a query expressions
        /// </summary>
        /// <returns>A sequence of data of the type of IQueryable</returns>
        public static IQueryable<T> Predicate<T>(this IDbConnection dbConnection)
        {
            return new DataQuery<T>(new QueryProvider(dbConnection));
        }

        /// <summary>
        /// Create a transaction for nested
        /// </summary>
        /// <param name="dbConnection"></param>
        //public static void BeginGlobalTransaction(this IDbConnection dbConnection)
        //{
        //    BeginNestedTransaction(dbConnection);
        //}

        /// <summary>
        /// Create a transaction for nested
        /// </summary>
        /// <param name="dbConnection"></param>
        public static void BeginNestedTransaction(this IDbConnection dbConnection)
        {
            var seq = dbConnection.GetHashCode();
            if (!deepCount.ContainsKey(seq))
            {
                deepCount.Add(seq, 0);
            }

            if (deepCount[seq] <= 0)
            {
                transaction = dbConnection.BeginTransaction();
            }
            deepCount[seq] += 1;
        }

        /// <summary>
        /// Commit a transaction for nested
        /// </summary>
        /// <param name="dbConnection"></param>
        public static void CommitNested(this IDbConnection dbConnection)
        {
            var seq = dbConnection.GetHashCode();
            deepCount[seq] -= 1;
            if (deepCount[seq] == 0)
            {
                transaction.Commit();
                deepCount.Remove(seq);
            }
        }

        /// <summary>
        /// Rollback a transaction for nested
        /// </summary>
        /// <param name="dbConnection"></param>
        public static void RollbackNested(this IDbConnection dbConnection)
        {
            var seq = dbConnection.GetHashCode();
            deepCount[seq] -= 1;
            if (deepCount[seq] == 0)
            {
                transaction.Rollback();
                deepCount.Remove(seq);
            }
        }

        /// <summary>
        /// Open the connection
        /// </summary>
        /// <param name="dbConnection"></param>
        public static void OpenConnection(this IDbConnection dbConnection)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
        }

        /// <summary>
        /// Close the connection
        /// </summary>
        /// <param name="dbConnection"></param>
        public static void CloseConnection(this IDbConnection dbConnection)
        {
            if (dbConnection.State != ConnectionState.Closed)
            {
                dbConnection.Close();
            }
        }
    }
}