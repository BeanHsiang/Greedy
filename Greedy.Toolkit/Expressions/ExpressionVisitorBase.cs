using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Expressions
{
    abstract class ExpressionVisitorBase : ExpressionVisitor
    {
        protected ExpressionVisitorContext Context { get; private set; }

        //public Token Token
        //{
        //    get;
        //    protected set;
        //}

        //public object ExtraObject
        //{
        //    get;
        //    protected set;
        //}

        internal ExpressionVisitorBase(ExpressionVisitorContext context)
        {
            this.Context = context;
        }
    }
}
