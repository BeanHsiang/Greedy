using Greedy.Toolkit.Sql;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Greedy.Dapper
{
    static partial class SqlMapper
    {
        private static TypeHandler typeHandler;
        private static ConcurrentDictionary<int, IDbTransaction> transactions = new ConcurrentDictionary<int, IDbTransaction>();
        private static ConcurrentDictionary<int, int> deepCount = new ConcurrentDictionary<int, int>();
        private static ConcurrentDictionary<int, Action<CallbackState>> callbackMap = new ConcurrentDictionary<int, Action<CallbackState>>();

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
        /// Update rows according to some unspecial condition expressions
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public static int Update<T>(this IDbConnection cnn, IDictionary<string, object> source, IDictionary<string, object> condion, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var sql = GetTypeHandler(cnn).GetUpdateSql<T>(source, condion);
            return cnn.Execute(sql, source.Concat(condion), transaction, commandTimeout);
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
            var count = deepCount.AddOrUpdate(seq, 0, (k, v) =>
            {
                return v + 1;
            });

            if (count == 0)
            {
                var transaction = dbConnection.BeginTransaction();
                transactions.TryAdd(seq, transaction);
            }
        }

        /// <summary>
        /// Commit a transaction for nested
        /// </summary>
        /// <param name="dbConnection"></param>
        public static void CommitNested(this IDbConnection dbConnection)
        {
            var seq = dbConnection.GetHashCode();
            int count;
            if (deepCount.TryGetValue(seq, out count))
            {
                if (count == 0)
                {
                    IDbTransaction transaction;
                    if (transactions.TryGetValue(seq, out transaction))
                    {
                        transaction.Commit();
                        deepCount.TryRemove(seq, out count);
                        transactions.TryRemove(seq, out transaction);
                    }
                }
                else
                {
                    deepCount.TryUpdate(seq, count - 1, count);
                }
            }
        }

        /// <summary>
        /// Rollback a transaction for nested
        /// </summary>
        /// <param name="dbConnection"></param>
        public static void RollbackNested(this IDbConnection dbConnection)
        {
            var seq = dbConnection.GetHashCode();
            int count;
            if (deepCount.TryGetValue(seq, out count))
            {
                if (count == 0)
                {
                    IDbTransaction transaction;
                    if (transactions.TryGetValue(seq, out transaction))
                    {
                        transaction.Rollback();
                        deepCount.TryRemove(seq, out count);
                        transactions.TryRemove(seq, out transaction);
                    }
                }
                else
                {
                    deepCount.TryUpdate(seq, count - 1, count);
                }
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
                var seq = dbConnection.GetHashCode();
                IDbTransaction transaction;
                if (transactions.TryGetValue(seq, out transaction))
                {
                    transaction.Rollback();
                    int count;
                    deepCount.TryRemove(seq, out count);
                    transactions.TryRemove(seq, out transaction);
                }

                dbConnection.Close();
            }
        }

        public static void Bind(this IDbConnection connection, Action<CallbackState> action)
        {
            callbackMap.TryAdd(connection.GetHashCode(), action);
        }

        public static void Unbind(this IDbConnection connection)
        {
            Action<CallbackState> action;
            callbackMap.TryRemove(connection.GetHashCode(), out action);
        }

        internal static void Callback(this IDbConnection connection, CommandDefinition command)
        {
            Action<CallbackState> action;
            if (callbackMap.TryGetValue(connection.GetHashCode(), out action))
            {
                action(new CallbackState { CommandText = command.CommandText, Parameter = command.Parameters });
            }
        }
    }

    public class CallbackState
    {
        public string CommandText { get; set; }

        public object Parameter { get; set; }
    }
}