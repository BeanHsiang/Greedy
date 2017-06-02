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
        private static ConcurrentDictionary<IDbConnection, Action<CallbackState>> callbackMap = new ConcurrentDictionary<IDbConnection, Action<CallbackState>>();

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
        public static int Insert<T>(this IDbConnection connection, object parameter, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var type = parameter.GetType();
            if (type.IsArray && !(parameter is IEnumerable<T>))
            {
                object[] arr = (object[])parameter;
                var count = 0;
                foreach (var item in arr)
                {
                    count += connection.Insert<T>(item, transaction, commandTimeout);
                }
                return count;
            }
            else
            {
                InitialKeyOfParameter<T>(parameter);
                return connection.Execute(GetTypeHandler(connection).GetInsertSql<T>(parameter), parameter, transaction, commandTimeout);
            }
        }

        /// <summary>
        /// Insert an object as per T
        /// </summary>
        /// <returns>THe identity value of last inserted row</returns>
        public static long InsertWithIdentity<T>(this IDbConnection connection, object parameter, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return connection.ExecuteScalar<long>(GetTypeHandler(connection).GetInsertSqlWithIdentity<T>(parameter), parameter, transaction, commandTimeout);
        }

        /// <summary>
        /// Update one or more objects as per T, that are the same type 
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public static int Update<T>(this IDbConnection connection, object parameter, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return connection.Execute(GetTypeHandler(connection).GetUpdateSql<T>(parameter), parameter, transaction, commandTimeout);
        }

        /// <summary>
        /// Update rows according to some unspecial condition expressions
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public static int Update<T>(this IDbConnection connection, IDictionary<string, object> source, IDictionary<string, object> condion, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var sql = GetTypeHandler(connection).GetUpdateSql<T>(source, condion);
            return connection.Execute(sql, source.Concat(condion), transaction, commandTimeout);
        }

        /// <summary>
        /// Update rows according to some special condition expressions
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public static int Update<T>(this IDbConnection connection, Expression<Func<T, bool>> expression, IDictionary<Expression<Func<T, object>>, object> parameter, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            IDictionary<string, dynamic> parameters;
            var sql = GetTypeHandler(connection).GetUpdateSql<T>(expression, parameter, out parameters);
            return connection.Execute(sql, parameters, transaction, commandTimeout);
        }

        /// <summary>
        /// Update rows according to a special condition expression
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public static int Update<T>(this IDbConnection connection, Expression<Func<T, bool>> expression, Expression<Func<T, object>> parameter, object value, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            IDictionary<string, dynamic> parameters;
            var sql = GetTypeHandler(connection).GetUpdateSql<T>(expression, parameter, value, out parameters);
            return connection.Execute(sql, parameters, transaction, commandTimeout);
        }

        /// <summary>
        /// Get a update-prepared set
        /// </summary>
        /// <returns>ColumnSet type</returns>
        public static ColumnSet<T> Set<T>(this IDbConnection connection)
        {
            return new ColumnSet<T>(connection);
        }

        /// <summary>
        /// Delete rows according to a special condition expression
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public static int Delete<T>(this IDbConnection connection, Expression<Func<T, bool>> expression, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            IDictionary<string, dynamic> parameters;
            var sql = GetTypeHandler(connection).GetDeleteSql<T>(expression, out parameters);
            return connection.Execute(sql, parameters, transaction, commandTimeout);
        }

        /// <summary>
        /// Execute a query, returning the data typed as per T
        /// </summary>
        /// <returns>A sequence of data of the supplied type</returns>
        public static IEnumerable<T> Get<T>(this IDbConnection connection, Expression<Func<T, bool>> expression, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null)
        {
            IDictionary<string, dynamic> parameters;

            var sql = GetTypeHandler(connection).GetFetchSql<T>(expression, out parameters);
            //cnn.Query<T>().Where(expression);
            return connection.Query<T>(sql, parameters, transaction, buffered, commandTimeout);
        }

        /// <summary>
        /// Get a query expressions
        /// </summary>
        /// <returns>A sequence of data of the type of IQueryable</returns>
        public static IQueryable<T> Predicate<T>(this IDbConnection connection)
        {
            return new DataQuery<T>(new QueryProvider(connection));
        }

        public static void Bind(this IDbConnection connection, Action<CallbackState> action)
        {
            callbackMap.TryAdd(connection, action);
        }

        public static void Unbind(this IDbConnection connection)
        {
            Action<CallbackState> action;
            callbackMap.TryRemove(connection, out action);
        }

        internal static void Callback(this IDbConnection connection, CommandDefinition command)
        {
            Action<CallbackState> action;
            if (callbackMap.TryGetValue(connection, out action))
            {
                action(new CallbackState { CommandText = command.CommandText, Parameter = command.Parameters });
            }
        }

        private static void InitialKeyOfParameter<T>(object parameter)
        {
            var type = typeof(T);
            var typeMapper = TypeMapperCache.GetTypeMapper(type);
            var seed = new Snowflake();
            if (typeMapper.Name == TypeMapper.Dictionary_Name)
            {
                var diction = parameter as IDictionary;
                foreach (var member in typeMapper.GetKeyMembers(false).Where(m => m.KeyType == KeyType.Snxowflake))
                {
                    if (!diction.Contains(member.Name))
                    {
                        diction.Add(member.Name, seed.GenerateId());
                    }
                    else
                    {
                        diction[member.Name] = seed.GenerateId();
                    }
                }
            }
            else
            {
                var instanceType = parameter.GetType();
                foreach (var member in typeMapper.GetKeyMembers(false).Where(m => m.KeyType == KeyType.Snxowflake))
                {
                    var property = instanceType.GetProperty(member.Name);
                    if (property == null)
                    {
                        //the property does not exist
                        return;
                    }
                    var setMethod = property.GetSetMethod();
                    if (setMethod == null)
                    {
                        //the property is readonly
                        return;
                    }
                    else
                    {
                        var instance = Expression.Constant(parameter);
                        var valueParameter = Expression.Parameter(property.PropertyType);
                        var method = Expression.Call(instance, setMethod, valueParameter);
                        var expression = Expression.Lambda<Action<long>>(method, valueParameter).Compile();
                        var nextValue = seed.GenerateId();
                        expression(nextValue);
                    }
                }
            }
        }
    }

    public class CallbackState
    {
        public string CommandText { get; set; }

        public object Parameter { get; set; }
    }
}