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
        public SingleCondition Condition { get; private set; }

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

        private SingleCondition GetRelationCondition(string relation, BinaryExpression node)
        {
            var condition = new SingleCondition();
            condition.Relation = relation;

            var leftVisitor = new BinaryExpressionVisitor(this.Context);
            leftVisitor.Visit(node.Left);
            condition.Left = leftVisitor.Column;

            var rightVisitor = new BinaryExpressionVisitor(this.Context);
            rightVisitor.Visit(node.Right);
            condition.Right = rightVisitor.Column;

            return condition;
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
            this.Column = new ParameterColumn(this.Context.AddParameter(node.Value));
            return node;
        }
    }
}