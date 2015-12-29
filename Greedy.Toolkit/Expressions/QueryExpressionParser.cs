using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Expressions
{
    class QueryExpressionParser : ExpressionVisitor
    {
        private IDbConnection connection;
        private ExpressionVisitorContext context;

        internal QueryExpressionParser(IDbConnection connection)
        {
            this.connection = connection;
            this.context = new ExpressionVisitorContext(connection);
        }

        internal void Parse(Expression expression)
        {
            this.Visit(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            switch (node.Method.Name)
            {
                case "Where":
                    var visitor = new WhereExpressionVisitor(context);
                    visitor.Visit(node);

                    break;
                default:
                    break;
            }
            return node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var elementType = typeof(T);
            if (elementType.IsGenericType && elementType.GetGenericArguments().Last() == typeof(bool))
            {
                var visitor = new WhereExpressionVisitor(context);
                visitor.Visit(node);
                context.Fragment.WherePart = visitor.Condition;
            }
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var visitor = new BinaryExpressionVisitor(context);
            visitor.Visit(node);

            return node;
        }

        internal string ToSql()
        {
            return context.ToSql();
        }

        internal IDictionary<string, object> Parameters { get { return context.Parameters; } }

        internal IDictionary<Type, string> TableNames { get { return context.TableNames; } }
    }
}
