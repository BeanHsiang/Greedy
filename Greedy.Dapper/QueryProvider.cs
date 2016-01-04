using Greedy.Toolkit.Expressions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Dapper
{
    public class QueryProvider : IQueryProvider
    {
        Expression _expression;
        Type _elementType;
        IDbConnection _connection;
        public QueryProvider(IDbConnection connection)
        {
            _connection = connection;
        }
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            _expression = expression;
            return new DataQuery<TElement>(this, expression);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = expression.Type.GetGenericArguments()[0];
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(DataQuery<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        public string GetCommandText(Expression expression)
        {
            var parser = new QueryExpressionParser(_connection);
            parser.Parse(expression);
            return parser.ToSql();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var parser = new QueryExpressionParser(_connection);
            parser.Parse(expression);

            Type type = typeof(TResult);

            if (expression.NodeType == ExpressionType.Call && type.IsValueType)
            {
                var method = ((MethodCallExpression)expression).Method;
                switch (method.Name)
                {
                    case "Any":
                    case "Average":
                    case "Sum":
                    case "Count":
                        var returnMethod = typeof(SqlMapper).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).FirstOrDefault(x => x.Name == "ExecuteScalar" && x.IsGenericMethodDefinition).MakeGenericMethod(type);
                        var param0 = Expression.Parameter(typeof(IDbConnection));
                        var param1 = Expression.Parameter(typeof(string));
                        var param2 = Expression.Parameter(typeof(object));
                        var param3 = Expression.Parameter(typeof(IDbTransaction));
                        var param4 = Expression.Parameter(typeof(int?));
                        var param5 = Expression.Parameter(typeof(CommandType?));
                        var source = Expression.Call(returnMethod, param0, param1, param2, param3, param4, param5);
                        return Expression.Lambda<Func<IDbConnection, string, object, IDbTransaction, int?, CommandType?, TResult>>(source, param0, param1, param2, param3, param4, param5).Compile()(_connection, parser.ToSql(), parser.Parameters, null, null, null);

                    default:
                        throw new Exception("not supported yet");
                }
            }
            else
            {
                MethodInfo method;
                //var isAnonymous = false;
                //if (!isAnonymous)
                //{
                method = typeof(SqlMapper).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).FirstOrDefault(x => x.Name == "Query" && x.IsGenericMethodDefinition).MakeGenericMethod(type.GetGenericArguments());
                //}
                //else
                //{
                //    method = typeof(SqlMapper).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).FirstOrDefault(x => x.Name == "Query" && !x.IsGenericMethodDefinition);
                //}
                var param0 = Expression.Parameter(typeof(IDbConnection));
                var param1 = Expression.Parameter(typeof(string));
                var param2 = Expression.Parameter(typeof(object));
                var param3 = Expression.Parameter(typeof(IDbTransaction));
                var param4 = Expression.Parameter(typeof(bool));
                var param5 = Expression.Parameter(typeof(int?));
                var param6 = Expression.Parameter(typeof(CommandType?));
                var source = Expression.Call(method, param0, param1, param2, param3, param4, param5, param6);
                return Expression.Lambda<Func<IDbConnection, string, object, IDbTransaction, bool, int?, CommandType?, TResult>>(source, param0, param1, param2, param3, param4, param5, param6).Compile()(_connection, parser.ToSql(), parser.Parameters, null, true, null, null);
            }
        }

        public object Execute(Expression expression)
        {
            var type = expression.Type;
            if (type.IsGenericType)
            {
                var typeDef = type.GetGenericTypeDefinition();
                if (typeDef == typeof(IQueryable<>))
                {
                    type = typeof(IEnumerable<>).MakeGenericType(type.GetGenericArguments());
                }
            }
            var param = Expression.Parameter(typeof(Expression));
            var expr = Expression.Call(Expression.Constant(this),
                this.GetType().GetMethods().FirstOrDefault(x => x.Name == "Execute" && x.IsGenericMethodDefinition).MakeGenericMethod(type),
                param);

            return Expression.Lambda<Func<Expression, object>>(expr, param).Compile()(expression);
        }
    }
}
