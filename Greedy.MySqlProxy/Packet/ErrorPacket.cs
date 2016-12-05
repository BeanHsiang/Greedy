using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.MySqlProxy.Packet
{
    class ErrorPacket : ResultPacket
    {
        public override byte Header { get { return 0xff; } }

        public short ErrorCode { get; set; }

        public string SqlStateMarker { get; set; }

        public string SqlState { get; set; }

        public string ErrorMessage { get; set; }

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
