using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Expressions
{
    class GroupByExpressionVisitor : ExpressionVisitorBase
    {
        public IList<Column> Columns { get; private set; }

        public GroupByExpressionVisitor(ExpressionVisitorContext context)
            : base(context)
        {
            Columns = new List<Column>();
        }

        public void Parse(Expression result)
        {
            Visit(result);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            for (int i = 0, len = node.Arguments.Count; i < len; i++)
            {
                var arg = node.Arguments[i];
                Visit(arg);
                this.Context.AddTempColumnMapper(new Tuple<Type, string, Column>(node.Members[i].DeclaringType, node.Members[i].Name, Columns.Last()));
            }
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            Visit(node.Operand);
            return node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Visit(node.Body);
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var visitor = new MemberExpressionVisitor(this.Context);
            visitor.Visit(node);
            Columns.Add(visitor.Column);
            return node;
        }
    }
}
