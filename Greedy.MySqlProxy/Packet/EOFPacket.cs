using Greedy.MySqlProxy.Util;
using System;
using System.IO;

namespace Greedy.MySqlProxy.Packet
{
    class EOFPacket : ResultPacket
    {
        public override byte Header { get { return 0xfe; } }

        public CapabilityFlags Capabilities { get; set; }

        public StatusFlags Status { get; set; }

        public short WarningsNumber { get; set; }

        protected override void ParseBody()
        {
            this.Body.Seek(1, SeekOrigin.Begin);

            if ((Capabilities & CapabilityFlags.CLIENT_PROTOCOL_41) > 0)
            {
                this.WarningsNumber = this.Body.ReadShort();
                this.Status = (StatusFlags)this.Body.ReadShort();
            }
        }

        protected override void BuildBody()
        {
            throw new NotImplementedException();
        }
    }
}
