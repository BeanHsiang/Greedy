using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Greedy.MySqlProxy.Util;

namespace Greedy.MySqlProxy.Packet
{
    class TextResultSetPacket
    {
        public ResultSetHeader Header { get; set; }

        public FieldPacket[] Fields { get; set; }

        public EOFPacket EOF { get; set; }

        public RowPacket[] Rows { get; set; }

        public CapabilityFlags Capabilities { get; set; }

        public ResultPacket Result { get; set; }

        public void Read(Socket socket)
        {
            this.Header = new ResultSetHeader();
            this.Header.Read(socket);

            this.Fields = new FieldPacket[this.Header.FieldPacketCount];

            for (int i = 0; i < this.Header.FieldPacketCount; i++)
            {
                this.Fields[i] = new FieldPacket();
                this.Fields[i].Read(socket);
            }

            if ((Capabilities & CapabilityFlags.CLIENT_DEPRECATE_EOF) == 0)
            {
                this.EOF = new EOFPacket();
                this.EOF.Read(socket);
            }

            var rows = new List<RowPacket>();
            ResultPacket packet;
            do
            {
                var packetData = socket.ReceiveAllBytes();
                packet = MySqlPacketFactory.GetResultPacket(packetData);
                if (packet == null)
                {
                    var row = new RowPacket();
                    row.Read(packetData.Data);
                    rows.Add(row);
                }
                else
                {
                    break;
                }
            } while (true);

            Result = packet;
        }
    }
}
