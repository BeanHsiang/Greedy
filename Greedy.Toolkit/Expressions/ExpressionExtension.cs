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

                    result = memberExpr.NodeType == ExpressionType.Constant || memberExpr.Type.IsValueType || (memberExpr.Expression != null && memberExpr.Expression.NodeType == ExpressionType.Constant);
                    break;
            }
            return result;
        }

        public static bool IsParameter(this Expression expr)
        {
            if (expr.NodeType == ExpressionType.Parameter)
                return true;
            if (expr.NodeType == ExpressionType.MemberAccess)
                return (expr as MemberExpression).Expression.IsParameter();
            return false;
        }

        public static ParameterExpression GetParameterExpresion(this Expression expr)
        {
            if (expr.NodeType == ExpressionType.Parameter)
                return expr as ParameterExpression;
            if (expr.NodeType == ExpressionType.MemberAccess)
                return (expr as MemberExpression).Expression.GetParameterExpresion();
            return null;
        }
    }
}
