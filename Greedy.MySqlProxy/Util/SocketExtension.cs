using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.MySqlProxy.Util
{
    class PacketData
    {
        public byte[] Data { get; set; }

        public int BodyLength { get; set; }
    }

    static class SocketExtension
    {
        public static PacketData ReceiveAllBytes(this Socket socket)
        {
            var ret = new List<byte>();
            var headBuffer = new byte[4];
            socket.Receive(headBuffer);
            var buffer = new byte[1024];
            var lenbyts = new byte[4];
            Array.Copy(headBuffer, 0, lenbyts, 0, 3);
            var len = BitConverter.ToInt32(lenbyts, 0);
            var length = len;
            while (len > 0)
            {
                var currentLength = socket.Receive(buffer);
                ret.AddRange(buffer.Take(currentLength));
                len -= currentLength;
            }
            return new PacketData() { Data = ret.ToArray(), BodyLength = length };
        }
    }
}
