using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Expressions
{
    class WhereExpressionVisitor : ExpressionVisitorBase
    {
        public Condition Condition { get; private set; }

        public WhereExpressionVisitor(ExpressionVisitorContext context)
            : base(context)
        {
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                    this.Condition = GetRelationCondition(" and ", node);
                    break;
                case ExpressionType.OrElse:
                    this.Condition = GetRelationCondition(" or ", node);
                    break;
                default:
                    var visitor = new BinaryExpressionVisitor(this.Context);
                    visitor.Visit(node);
                    this.Condition = visitor.Condition;
                    break;
            }
            return node;
        }

        private Condition GetRelationCondition(string relation, BinaryExpression node)
        {
            var condition = new GroupCondition();
            condition.Relation = relation;

            var leftVisitor = new WhereExpressionVisitor(this.Context);
            leftVisitor.Visit(node.Left);
            condition.Left = leftVisitor.Condition;

            var rightVisitor = new WhereExpressionVisitor(this.Context);
            rightVisitor.Visit(node.Right);
            condition.Right = rightVisitor.Condition;

            return condition;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var visitor = new MethodCallExpressionVisitor(this.Context);
            visitor.Visit(node);
            this.Condition = new SingleCondition { Right = visitor.Column };
            return node;
        }
    }
}