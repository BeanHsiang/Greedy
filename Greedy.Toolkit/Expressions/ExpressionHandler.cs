using Greedy.Toolkit.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Greedy.Toolkit.Expressions
{
    class ExpressionHandler
    {
        public TypeHandler TypeHandler { get; private set; }
        public ExpressionContext Context { get; private set; }
        public ExpressionHandleOption Option { get; private set; }

        internal ExpressionHandler(ExpressionContext context, ExpressionHandleOption option)
        {
            TypeHandler = context.TypeHandler;
            Option = option;
            Context = context;
        }

        private string GetSql(Expression expression)
        {
            if (expression is LambdaExpression)
            {
                return GetSql(expression as LambdaExpression);
            }
            else if (expression is ConstantExpression)
            {
                return GetSql(expression as ConstantExpression);
            }
            else if (expression is BinaryExpression)
            {
                return GetSql(expression as BinaryExpression);
            }
            else if (expression is MemberExpression)
            {
                return GetSql(expression as MemberExpression);
            }
            else if (expression is UnaryExpression)
            {
                return GetSql(expression as UnaryExpression);
            }
            else if (expression is MethodCallExpression)
            {
                return GetSql(expression as MethodCallExpression);
            }
            else if (expression is NewArrayExpression)
            {
                return GetSql(expression as NewArrayExpression);
            }
            else if (expression is NewExpression)
            {
                return GetSql(expression as NewExpression);
            }
            else if (expression is ParameterExpression)
            {
                return GetSql(expression as ParameterExpression);
            }
            return string.Empty;
        }

        public string GetSql(LambdaExpression expression)
        {
            InitParameters(expression.Parameters);
            return GetSql(expression.Body);
        }

        public string GetSql(IEnumerable<LambdaExpression> expressions)
        {
            return expressions.Select(e => GetSql(e)).Aggregate((f, s) => string.Format("{0};{1}", f, s));
        }

        public string GetSql(ConstantExpression expression)
        {
            return Context.AddParameter(null, expression.Value);
        }

        public string GetSql(ConstantExpression expression, MemberInfo memberInfo)
        {
            return Context.AddParameter(null, expression.Value);
        }

        public string GetSql(NewExpression expression)
        {
            if (expression.Arguments.Count == 1)
                return GetSql(expression.Arguments[0]);
            return null;
        }

        public string GetSql(NewArrayExpression expression)
        {
            var sb = new StringBuilder();
            sb.Append("(");
            sb = expression.Expressions.Aggregate(sb, (s, e) => s.AppendFormat("{0},", GetSql(e)));
            sb.Remove(sb.Length - 1, 1);
            sb.Append(")");
            return sb.ToString();
        }

        public string GetSql(UnaryExpression expression)
        {
            return GetSql(expression.Operand);
        }

        public void InitParameters(IEnumerable<ParameterExpression> expressions)
        {
            foreach (var expr in expressions)
            {
                Context.AddAlias(expr.Type);
            }
        }

        public string GetSql(ParameterExpression expression, MemberInfo memberInfo)
        {
            var typeMapper = TypeHandler.GetTypeMapper(expression.Type);
            var columnMapper = typeMapper.AllMembers.SingleOrDefault(m => m.Name == memberInfo.Name);
            if (Option.UseAlias)
            {
                return string.Format("{0}.{1}", Context.GetAlias(typeMapper), TypeHandler.SqlGenerator.DecorateName(columnMapper.ColumnName));
            }
            else
            {
                return TypeHandler.SqlGenerator.DecorateName(columnMapper.ColumnName);
            }
        }

        public object GetObject(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant)
            {
                return (expression as ConstantExpression).Value;
            }
            else if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var exp = expression as MemberExpression;
                var r = exp.Member.DeclaringType.GetField(exp.Member.Name).GetValue(GetObject(exp.Expression));
                return r;
            }
            return "";
        }

        public string GetSql(MemberExpression expression)
        {
            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                return GetSql(expression.Expression as ParameterExpression, expression.Member);
            }
            else
            {
                var obj = GetObject(expression.Expression);
                object r = null;
                var field = obj.GetType().GetField(expression.Member.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field == null)
                {
                    var property = obj.GetType().GetProperty(expression.Member.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
#if NET45
                    r = property.GetValue(obj);
#else
                    r = property.GetValue(obj, null);
#endif

                }
                else
                    r = field.GetValue(obj);
                return Context.AddParameter(null, r);
            }

        }

        public string GetSql(MethodCallExpression expression)
        {
            Expression arg0, arg1;

            if (expression.Method.Name == "Contains")
            {
                if (expression.Object == null)
                {
                    arg0 = expression.Arguments[0];
                    arg1 = expression.Arguments[1];
                }
                else
                {
                    arg0 = expression.Object;
                    arg1 = expression.Arguments[0];
                }

                if (arg0.Type != typeof(string)) // arg1.NodeType == ExpressionType.MemberAccess
                {
                    return string.Format("{0} in {1}", GetSql(arg1), GetSql(arg0));
                }
                else
                {
                    return string.Format("LOCATE({1}, {0}) > 0 ", GetSql(arg0), GetSql(arg1));
                    //return string.Format("{0} like {1}", GetSql(expression.Object), Context.AddParameter(null, "%" + (expression.Arguments[0] as ConstantExpression).Value + "%"));
                }
            }
            else if (expression.Method.Name == "Equals")
            {
                if (expression.Object == null)
                {
                    arg0 = expression.Arguments[0];
                    arg1 = expression.Arguments[1];
                }
                else
                {
                    arg0 = expression.Object;
                    arg1 = expression.Arguments[0];
                }

                return string.Format("{0} = {1}", GetSql(arg0), GetSql(arg1));
            }
            else if (expression.Method.Name == "IndexOf")
            {
                if (expression.Object == null)
                {
                    arg0 = expression.Arguments[0];
                    arg1 = expression.Arguments[1];
                }
                else
                {
                    arg0 = expression.Object;
                    arg1 = expression.Arguments[0];
                }

                return string.Format("LOCATE({1}, {0})", GetSql(arg0), GetSql(arg1));
            }
            else if (expression.Method.Name == "Parse")
            {
                if (expression.Object == null)
                {
                    arg0 = expression.Arguments[0];
                }
                else
                {
                    arg0 = expression.Object;
                }
                return GetSql(arg0);
            }
            return string.Empty;
        }

        public string GetSql(BinaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.AndAlso)
            {
                return string.Format("({0}) and ({1}) ", GetSql(expression.Left), GetSql(expression.Right));
            }
            else if (expression.NodeType == ExpressionType.OrElse)
            {
                return string.Format("({0}) or ({1}) ", GetSql(expression.Left), GetSql(expression.Right));
            }
            else if (expression.NodeType == ExpressionType.Equal)
            {
                return string.Format("{0} = {1} ", GetSql(expression.Left), GetSql(expression.Right));
            }
            else if (expression.NodeType == ExpressionType.NotEqual)
            {
                return string.Format("{0} <> {1} ", GetSql(expression.Left), GetSql(expression.Right));
            }
            else if (expression.NodeType == ExpressionType.GreaterThan)
            {
                return string.Format("{0} > {1} ", GetSql(expression.Left), GetSql(expression.Right));
            }
            else if (expression.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                return string.Format("{0} >= {1} ", GetSql(expression.Left), GetSql(expression.Right));
            }
            else if (expression.NodeType == ExpressionType.LessThan)
            {
                return string.Format("{0} < {1} ", GetSql(expression.Left), GetSql(expression.Right));
            }
            else if (expression.NodeType == ExpressionType.LessThanOrEqual)
            {
                return string.Format("{0} <= {1} ", GetSql(expression.Left), GetSql(expression.Right));
            }
            else if (expression.NodeType == ExpressionType.Add)
            {
                return string.Format("{0} + {1} ", GetSql(expression.Left), GetSql(expression.Right));
            }
            else if (expression.NodeType == ExpressionType.Subtract)
            {
                return string.Format("{0} - {1} ", GetSql(expression.Left), GetSql(expression.Right));
            }
            else if (expression.NodeType == ExpressionType.ArrayIndex)
            {
                var r = GetObject(expression.Left);

                var param = r.GetType().GetMethod("GetValue", new Type[] { typeof(int) }).Invoke(r, new[] { GetObject(expression.Right) });
                return Context.AddParameter(null, param);
            }
            return "";
        }
    }
}
