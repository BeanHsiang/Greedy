
namespace Greedy.Toolkit.Sql
{
    class MySqlDialect : SqlDialectBase
    {
        public override char LeftQuote
        {
            get { return '`'; }
        }

        public override char RightQuote
        {
            get { return '`'; }
        }

        public override string GetIdentitySql()
        {
            return "SELECT LAST_INSERT_ID() AS ID";
        }
    }
}
