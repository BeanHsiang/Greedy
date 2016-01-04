using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Expressions
{
    static class ExpressionExtension
    {
        public static bool IsConstant(this Expression expr)
        {
            var result = false;
            switch (expr.NodeType)
            {
                case ExpressionType.Constant:
                    result = true;
                    break;
                case ExpressionType.Call:
                    var methodExpr = expr as MethodCallExpression;
                    result = methodExpr.Arguments.Aggregate(methodExpr.Object != null && methodExpr.Object.IsConstant(), (r, e) => r && e.IsConstant());
                    break;
                case ExpressionType.MemberAccess:
                    var memberExpr = expr as MemberExpression;

                    result = memberExpr.NodeType == ExpressionType.Constant || (memberExpr.Expression.NodeType == ExpressionType.Constant && memberExpr.Type.IsValueType);
                    break;
            }
            return result;
        }
    }
}
