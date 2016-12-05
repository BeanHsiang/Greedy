using Greedy.MySqlProxy.Packet;
using Greedy.MySqlProxy.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.MySqlProxy
{
    class MySqlPacketFactory
    {
        public static ResultPacket GetResultPacket(Socket socket)
        {
            var packetData = socket.ReceiveAllBytes();

            return GetResultPacket(packetData);
        }

        public static ResultPacket GetResultPacket(PacketData packetData)
        {
            ResultPacket resultPacket = null;
            if (packetData.Data[4] == 0xfe && packetData.BodyLength <= 9)
            {
                resultPacket = new EOFPacket();
            }
            else if (packetData.Data[4] == 0x00)
            {
                resultPacket = new OKPacket();
            }
            else if (packetData.Data[4] == 0xff)
            {
                resultPacket = new ErrorPacket();
            }

            if (resultPacket != null)
            {
                resultPacket.Read(packetData.Data);
            }
            return resultPacket;
        }
    }
}
