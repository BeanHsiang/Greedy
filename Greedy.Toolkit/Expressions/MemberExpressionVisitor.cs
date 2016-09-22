using Greedy.Toolkit.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Expressions
{
    class MemberExpressionVisitor : ExpressionVisitorBase
    {
        public Column Column { get; private set; }

        public bool UseColumnAlias { get; set; }

        internal MemberExpressionVisitor(ExpressionVisitorContext context)
            : base(context)
        {
            UseColumnAlias = false;
        }

        internal MemberExpressionVisitor(ExpressionVisitorContext context, bool useColumnAlias)
            : base(context)
        {
            UseColumnAlias = useColumnAlias;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null && node.Expression.IsParameter())
            {
                var parameterExpresion = node.Expression.GetParameterExpresion();
                var memberType = node.Member.DeclaringType;
                if (parameterExpresion != null && parameterExpresion.Type.IsSubclassOf(memberType))
                {
                    // memberType = parameterExpresion.Type;
                    TypeMapperCache.AddTransferTypeMapper(memberType, parameterExpresion.Type);
                }

                if (node.Member.MemberType == MemberTypes.Property)
                {
                    var property = node.Member as PropertyInfo;
                    if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        memberType = property.PropertyType.GenericTypeArguments.First();
                        this.Column = new MemberColumn("*", null, UseColumnAlias ? node.Member.Name : null) { Type = node.Type };
                        return node;
                    }
                }

                var tempColumn = this.Context.GetMappedColumn(memberType, node.Member.Name);
                if (tempColumn == null)
                {
                    var typeMapper = TypeMapperCache.GetTypeMapper(memberType);
                    var columnMapper = typeMapper.AllMembers.SingleOrDefault(m => m.Name == node.Member.Name);
                    this.Column = new MemberColumn(columnMapper.ColumnName, Context.GetTableAlias(TypeMapperCache.GetTransferTypeMapper(memberType)), UseColumnAlias ? node.Member.Name : null) { Type = node.Type };
                }
                else
                {
                    this.Column = tempColumn;
                }
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

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Type.IsGenericType && node.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var type = node.Type.GenericTypeArguments.First();
                this.Column = new MemberColumn("*", null, UseColumnAlias ? node.Name : null) { Type = node.Type };
                return node;
            }
            return node;
        }
    }
}
