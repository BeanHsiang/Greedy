using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Expressions
{
    class SelectExpressionVisitor : ExpressionVisitorBase
    {
        public ICollection<Column> Columns { get; private set; }

        public SelectExpressionVisitor(ExpressionVisitorContext context)
            : base(context)
        {
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            Visit(node.Operand);
            return node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var elementType = node.ReturnType;
            Context.ExportType = elementType;
            Visit(node.Body);
            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            if (node.Arguments.Count == 0)
                return node;
            Columns = new List<Column>();
            for (var i = 0; i < node.Arguments.Count; i++)
            {
                var arg = node.Arguments[i];
                var memberVisitor = new MemberExpressionVisitor(this.Context);
                memberVisitor.Visit(arg);
                memberVisitor.Column.Alias = node.Members[i].Name;
                Columns.Add(memberVisitor.Column);
            }
            return node;
        }
    }
}
