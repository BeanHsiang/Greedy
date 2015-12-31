using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Expressions
{
    class BinaryExpressionVisitor : ExpressionVisitorBase
    {
        public Condition Condition { get; private set; }

        public Column Column { get; private set; }

        internal BinaryExpressionVisitor(ExpressionVisitorContext context)
            : base(context)
        { }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var relation = string.Empty;
            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    relation = " = ";
                    break;
                case ExpressionType.NotEqual:
                    relation = " <> ";
                    break;
                case ExpressionType.GreaterThan:
                    relation = " > ";
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    relation = " >= ";
                    break;
                case ExpressionType.LessThan:
                    relation = " < ";
                    break;
                case ExpressionType.LessThanOrEqual:
                    relation = " <= ";
                    break;
                case ExpressionType.Add:
                    relation = " + ";
                    break;
                case ExpressionType.Subtract:
                    relation = " - ";
                    break;
                default:
                    break;
            }
            this.Condition = GetRelationCondition(relation, node);
            return node;
        }

        private Condition GetRelationCondition(string relation, BinaryExpression node)
        {
            var leftVisitor = new BinaryExpressionVisitor(this.Context);
            leftVisitor.Visit(node.Left);

            var rightVisitor = new BinaryExpressionVisitor(this.Context);
            rightVisitor.Visit(node.Right);

            if (leftVisitor.Column != null && rightVisitor.Column != null)
            {
                var condition = new SingleCondition();
                condition.Left = leftVisitor.Column;
                condition.Right = rightVisitor.Column;
                condition.Relation = relation;
                return condition;
            }
            var group = new GroupCondition();
            group.Left = leftVisitor.Condition ?? new SingleCondition() { Right = leftVisitor.Column };
            group.Right = rightVisitor.Condition ?? new SingleCondition() { Right = rightVisitor.Column };
            group.Relation = relation;
            return group;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var visitor = new MemberExpressionVisitor(this.Context);
            visitor.Visit(node);
            Column = visitor.Column;
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var visitor = new MemberExpressionVisitor(this.Context);
            visitor.Visit(node);
            Column = visitor.Column;
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var visitor = new MethodCallExpressionVisitor(this.Context);
            visitor.Visit(node);
            Column = visitor.Column;
            return node;
        }
    }
}