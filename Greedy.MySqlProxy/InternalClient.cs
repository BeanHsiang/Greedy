using Greedy.MySqlProxy.Packet;
using Greedy.MySqlProxy.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Greedy.MySqlProxy
{
    class InternalClient
    {
        public Socket socket { get; private set; }

        string server;
        int port = 3306;
        string userName;
        string password;
        string database;

        internal InternalClient(string server, int port, string username, string password, string database)
        {
            this.server = server;
            this.port = port;
            this.userName = username;
            this.password = password;
            this.database = database;
        }

        internal InternalClient(string server, string username, string password, string database)
        {
            this.server = server;
            this.userName = username;
            this.password = password;
            this.database = database;
        }

        public void Connect()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ip = IPAddress.Parse(server);
            var ipEnd = new IPEndPoint(ip, port);

            try
            {
                socket.Connect(ipEnd);

            }
            catch (SocketException)
            {
                return;
            }
        }

        public ResultPacket Login()
        {
            var greeting = ReceiveGreeting();
            var challenge = greeting.AuthPluginDataPart1.Concat(greeting.AuthPluginDataPart2).ToArray();
            var sign = CalcChallenge(this.userName, this.password, challenge);

            var request = new HandshakeResponse41Packet()
            {
                MaxPacketSize = 0x40000000,
                Capabilities = (CapabilityFlags)0x000fa285,
                UserName = userName,
                Database = database,
                AuthResponse = sign,
                CharacterSet = CharacterSet.UTF8_GENERAL_CI,
                AuthPluginName = "mysql_native_password",
                Sequence = 1
            };
            request.Write(this.socket);

            return MySqlPacketFactory.GetResultPacket(this.socket);
        }

        private byte[] CalcChallenge(string userName, string password, byte[] salt)
        {
            var sha1Pwd = Crypto.Sha1(password);

            var calcSign = sha1Pwd.Xor(Crypto.Sha1(salt.Concat(Crypto.Sha1(sha1Pwd)).ToArray()));
            return calcSign;
        }

        private HandShakePacket ReceiveGreeting()
        {
            var handShakePacket = new HandShakePacket();
            handShakePacket.Read(this.socket);
            return handShakePacket;
        }

        public void SendCommand(CommandPacket comPacket)
        {
            comPacket.Write(this.socket);
        }
    }
}
