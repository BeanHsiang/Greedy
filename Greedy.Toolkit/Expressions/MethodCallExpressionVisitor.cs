using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Expressions
{
    class MethodCallExpressionVisitor : ExpressionVisitorBase
    {
        public Condition Condition { get; private set; }

        public Column Column { get; private set; }

        internal MethodCallExpressionVisitor(ExpressionVisitorContext context)
            : base(context)
        { }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return base.VisitMethodCall(node);
        }
    }
}

      //public string GetSql(MethodCallExpression expression)
      //  {
      //      Expression arg0, arg1;

      //      if (expression.Method.Name == "Contains")
      //      {
      //          if (expression.Object == null)
      //          {
      //              arg0 = expression.Arguments[0];
      //              arg1 = expression.Arguments[1];
      //          }
      //          else
      //          {
      //              arg0 = expression.Object;
      //              arg1 = expression.Arguments[0];
      //          }

      //          if (arg0.Type != typeof(string)) // arg1.NodeType == ExpressionType.MemberAccess
      //          {
      //              return string.Format("{0} in {1}", GetSql(arg1), GetSql(arg0));
      //          }
      //          else
      //          {
      //              return string.Format("LOCATE({1}, {0}) > 0 ", GetSql(arg0), GetSql(arg1));
      //              //return string.Format("{0} like {1}", GetSql(expression.Object), Context.AddParameter(null, "%" + (expression.Arguments[0] as ConstantExpression).Value + "%"));
      //          }
      //      }
      //      else if (expression.Method.Name == "Equals")
      //      {
      //          if (expression.Object == null)
      //          {
      //              arg0 = expression.Arguments[0];
      //              arg1 = expression.Arguments[1];
      //          }
      //          else
      //          {
      //              arg0 = expression.Object;
      //              arg1 = expression.Arguments[0];
      //          }

      //          return string.Format("{0} = {1}", GetSql(arg0), GetSql(arg1));
      //      }
      //      else if (expression.Method.Name == "IndexOf")
      //      {
      //          if (expression.Object == null)
      //          {
      //              arg0 = expression.Arguments[0];
      //              arg1 = expression.Arguments[1];
      //          }
      //          else
      //          {
      //              arg0 = expression.Object;
      //              arg1 = expression.Arguments[0];
      //          }

      //          return string.Format("LOCATE({1}, {0})", GetSql(arg0), GetSql(arg1));
      //      }
      //      else if (expression.Method.Name == "Parse")
      //      {
      //          if (expression.Object == null)
      //          {
      //              arg0 = expression.Arguments[0];
      //          }
      //          else
      //          {
      //              arg0 = expression.Object;
      //          }
      //          return GetSql(arg0);
      //      }
      //      return string.Empty;
      //  }
