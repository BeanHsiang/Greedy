using Greedy.Toolkit.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Expressions
{
    class MemberExpressionVisitor : ExpressionVisitorBase
    {
        public Column Column { get; private set; }

        internal MemberExpressionVisitor(ExpressionVisitorContext context)
            : base(context)
        { }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression.NodeType == ExpressionType.Parameter)
            {
                var type = node.Member.DeclaringType;
                var typeMapper = TypeMapperCache.GetTypeMapper(type);
                var columnMapper = typeMapper.AllMembers.SingleOrDefault(m => m.Name == node.Member.Name);
                this.Column = new MemberColumn(columnMapper.ColumnName, Context.GetTableAlias(type)) { Type = node.Type };
            }
            else
            {
                var objectMember = Expression.Convert(node, typeof(object));
                var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                var getter = getterLambda.Compile()();
                this.Column = new ParameterColumn(this.Context.AddParameter(getter)) { Type = node.Type };
            }
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            this.Column = new ParameterColumn(this.Context.AddParameter(node.Value)) { Type = node.Type };
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
