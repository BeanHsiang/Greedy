using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Dapper.Configuration
{
    class SnowflakeElement : ConfigurationElement
    {
        [ConfigurationProperty("workerid")]
        public long WorkerId
        {
            get
            {
                return Convert.ToInt64(this["workerid"]);
            }
            set
            {
                this["workerid"] = value;
            }
        }

        [ConfigurationProperty("datacenterid")]
        public long DataCenterId
        {
            get
            {
                return Convert.ToInt64(this["datacenterid"]);
            }
            set
            {
                this["datacenterid"] = value;
            }
        }

    }
}
