using Greedy.Toolkit.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public string GetSql(MemberExpression expression)
        {
            var typeMapper = TypeHandler.GetTypeMapper(expression.Expression.Type);
            var columnMapper = typeMapper.AllMembers.SingleOrDefault(m => m.Name == expression.Member.Name);
            if (Option.UseAlias)
            {
                return string.Format("{0}.{1}", Context.GetAlias(typeMapper), TypeHandler.SqlGenerator.DecorateName(columnMapper.ColumnName));
            }
            else
            {
                return TypeHandler.SqlGenerator.DecorateName(columnMapper.ColumnName);
            }

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
            return "";
        }
    }
}
