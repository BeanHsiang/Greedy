using System;

namespace Greedy.Toolkit.Sql
{
    interface ISqlDialect
    {
        char LeftQuote { get; }
        char RightQuote { get; }
        char ParameterPrefix { get; }
        string BatchSeperator { get; }
        string GetIdentitySql();
    }

    abstract class SqlDialectBase : ISqlDialect
    {
        public virtual char LeftQuote
        {
            get { return '"'; }
        }

        public virtual char RightQuote
        {
            get { return '"'; }
        }

        public virtual char ParameterPrefix
        {
            get
            {
                return '@';
            }
        }

        public virtual string BatchSeperator
        {
            get { return ";" + Environment.NewLine; }
        }

        public abstract string GetIdentitySql();
    }
}
