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
        public Column Column { get; private set; }

        internal MethodCallExpressionVisitor(ExpressionVisitorContext context)
            : base(context)
        { }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!node.IsConstant())
            {
                switch (node.Method.Name)
                {
                    case "Contains":
                        ParseMethodContains(node);
                        break;
                    case "Count":
                        ParseMethodCount(node);
                        break;
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

        private void ParseMethodContains(MethodCallExpression node)
        {
            var column = new FunctionColumn();
            if (node.Object.NodeType == ExpressionType.MemberAccess || node.Object.NodeType == ExpressionType.Constant)
            {
                var objectVisitor = new MemberExpressionVisitor(Context);
                objectVisitor.Visit(node.Object);
                if (objectVisitor.Column.Type == typeof(string))
                {
                    column.Formatter = "LOCATE({1}, {0}) > 0";
                }
                else
                {
                    column.Formatter = "{0} in {1}";
                }

                column.Add(objectVisitor.Column);

                var paramVisitor = new MemberExpressionVisitor(Context);
                paramVisitor.Visit(node.Arguments[0]);
                column.Add(paramVisitor.Column);
                this.Column = column;
            }
        }

        private void ParseMethodCount(MethodCallExpression node)
        {
            var column = new FunctionColumn();
            column.Formatter = "Count(*)";

            //var paramVisitor = new MemberExpressionVisitor(Context);
            //paramVisitor.Visit(node.Arguments[0]);
            //column.Add(paramVisitor.Column);
            this.Column = column;
            this.Column.Alias = (node.Arguments[0] as ParameterExpression).Name;
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
