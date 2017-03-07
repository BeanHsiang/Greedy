using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Dapper
{
    public class GreedyConnectionOption
    {
        public GreedyConnectionFlag Flags { get; set; }

        public long? OrgId { get; set; }
    }


    public enum GreedyConnectionFlag
    {
        ReadOnly = 1,
        ReadWrite,
        WriteOnly
    }
}
