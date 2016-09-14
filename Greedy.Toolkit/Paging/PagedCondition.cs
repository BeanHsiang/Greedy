using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.Paging
{
    public class PagedCondition
    {
        public IDictionary<string, dynamic> Filter { get; set; }
        public IDictionary<string, QueryOrder> Order { get; set; }
        public PagedParameter PagedParameter { get; set; }

        public PagedCondition()
        {
            this.PagedParameter = new PagedParameter();
        }

        public string ToLimitSql(Dictionary<string, dynamic> input)
        {
            if (this.PagedParameter == null)
            {
                return string.Empty;
            }
            input.Add("mysql_offset", PagedParameter.PageIndex - 1);
            input.Add("mysql_limit", PagedParameter.PageSize);
            return string.Format("limit @mysql_offset,@mysql_limit");
        }

        public string ToWhereSql(Func<IDictionary<string, dynamic>, string> func)
        {
            if (func == null || this.Filter == null)
            {
                return string.Empty;
            }
            return func(Filter);
        }

        public string ToOrderSql(Func<IDictionary<string, QueryOrder>, string> func)
        {
            if (func == null || this.Order == null)
            {
                return string.Empty;
            }
            return func(Order);
        }
    }

    public enum QueryOrder
    {
        ASC,
        DESC
    }
}
