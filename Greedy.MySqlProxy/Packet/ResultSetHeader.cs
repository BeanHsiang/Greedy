using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.MySqlProxy.Packet
{
    class ResultSetHeader : MySqlPacket
    {
        public int FieldPacketCount { get; set; }

        protected override void ParseBody()
        {
            throw new NotImplementedException();
        }

        protected override void BuildBody()
        {
            throw new NotImplementedException();
        }
    }
}
