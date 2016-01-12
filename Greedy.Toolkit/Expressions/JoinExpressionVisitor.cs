using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Expressions
{
    class JoinExpressionVisitor : ExpressionVisitorBase
    {
        public Condition Condition { get; set; }

        private IList<Column> leftColumns;
        private IList<Column> rightColumns;
        private short step;
        private Column currentColumn;

        public JoinExpressionVisitor(ExpressionVisitorContext context)
            : base(context)
        {
            leftColumns = new List<Column>();
            rightColumns = new List<Column>();
        }

        public void Parse(Expression left, Expression right, Expression result)
        {
            step = 1;
            Visit(left);
            step = 2;
            Visit(right);


            Condition lastCondition = new SingleCondition() { Left = leftColumns.First(), Relation = " = ", Right = rightColumns.First() };
            for (int i = 1, len = leftColumns.Count; i < len; i++)
            {
                var groupCondition = new GroupCondition() { Left = lastCondition, Relation = " and " };
                var condition = new SingleCondition();
                condition.Left = leftColumns[i];
                condition.Relation = " = ";
                condition.Right = rightColumns[i];
                groupCondition.Right = condition;
                lastCondition = groupCondition;
            }
            this.Condition = lastCondition;
            step = 3;
            Visit(result);
        }

        public void Parse(Expression result)
        {
            step = 3;
            Visit(result);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            for (int i = 0, len = node.Arguments.Count; i < len; i++)
            {
                var arg = node.Arguments[i];
                Visit(arg);
                if (step == 3 && currentColumn != null)
                {
                    this.Context.AddTempColumnMapper(new Tuple<Type, string, Column>(node.Members[i].DeclaringType, node.Members[i].Name, currentColumn));
                }
            }
            if (step == 3)
                this.Context.ExportType = node.Type;
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
            if (step == 1)
            {
                leftColumns.Add(visitor.Column);
            }
            else if (step == 2)
            {
                rightColumns.Add(visitor.Column);
            }
            else
            {
                //visitor.Column.Alias = node.Member.Name;
                currentColumn = visitor.Column;
            }
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var visitor = new MethodCallExpressionVisitor(this.Context);
            visitor.Visit(node);
            currentColumn = visitor.Column;
            return node;
        }
    }
}
