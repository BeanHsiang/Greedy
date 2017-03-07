using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Dapper.Configuration
{
    class GreedyDapperSection : ConfigurationSection
    {
        [ConfigurationProperty("snowflake")]
        public SnowflakeElement Snowflake
        {
            get
            {
                return this["snowflake"] as SnowflakeElement;
            }
            set
            {
                this["snowflake"] = value;
            }
        }
    }
}