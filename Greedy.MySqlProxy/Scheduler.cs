using Greedy.MySqlProxy.Packet;
using Greedy.MySqlProxy.Util;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace Greedy.MySqlProxy
{
    class Scheduler
    {
        internal Socket Socket { get; private set; }

        byte seq = 0;

        string username = "root";
        string pwd = "123123";
        byte[] plugData;

        public void Invoke(Socket socket)
        {
            this.Socket = socket;

            //发送请求握的包
            SendHandShake();
            //接收登录鉴权包，发送鉴权结果
            ReceiveHandShake();

            var isStop = false;
            do
            {
                //等待接收命令包
                var comPacket = ReceiveCommand();

                //根据解析出来的SQL，连接匹配的数据库
                var client = new InternalClient("192.168.5.203", "xiaobao_dev", "123456", "xiaobao_dev");
                client.Connect();
                if (client.Login() is OKPacket)
                {
                    //转发到真实数据库
                    client.SendCommand(comPacket);

                    //转发结果到客户端
                }
                else
                {

                }
                //var data = new byte[1024];
            } while (this.Socket.Connected && !isStop);

            //返回数据
        }

        private byte[] GetRandomChallenge()
        {
            var challenge = new byte[20];
            var rand = new Random(Thread.CurrentThread.ManagedThreadId);
            rand.NextBytes(challenge);
            return challenge;
        }

        private bool VerifyChallenge(string userName, byte[] sign)
        {
            //通过用户名找到密码和盐


            var sha1Pwd = Crypto.Sha1(pwd);

            var calcSign = sha1Pwd.Xor(Crypto.Sha1(plugData.Concat(Crypto.Sha1(sha1Pwd)).ToArray()));
            return calcSign.Compare(sign);
        }

        private void SendHandShake()
        {
            plugData = GetRandomChallenge();

            var packet = new HandShakePacket()
            {
                ServerVersion = "Greedy.MySqlProxy-0.1.0",
                ConnectionId = Thread.CurrentThread.ManagedThreadId,
                AuthPluginDataPart1 = plugData.Take(8).ToArray(),
                Capabilities = (CapabilityFlags)0xf7ff,
                CharacterSet = CharacterSet.UTF8_GENERAL_CI,
                Status = StatusFlags.SERVER_STATUS_AUTOCOMMIT,
                AuthPluginDataPart2 = plugData.Skip(8).Concat(new byte[] { 0x00 }).ToArray(),
                AuthPluginDataLength = 0x00,
                AuthPluginName = "mysql_native_password"
            };

            SendPacket(packet);
        }

        private void ReceiveHandShake()
        {
            var request = new HandshakeResponse41Packet();
            ReceivePacket(request);
            MySqlPacket response = null;
            if (VerifyChallenge(request.UserName, request.AuthResponse))
            {
                response = new OKPacket()
              {
                  AffectedRows = 0,
                  LastInsertId = 0,
                  Status = StatusFlags.SERVER_STATUS_AUTOCOMMIT,
                  Capabilities = CapabilityFlags.CLIENT_PROTOCOL_41,
                  Info = string.Empty
              };
            }
            else { }

            SendPacket(response);
        }

        private void SendPacket(MySqlPacket packet)
        {
            packet.Sequence = (byte)seq;
            packet.Write(this.Socket);
            seq++;
        }

        private void ReceivePacket(MySqlPacket packet)
        {
            //检查序列 
            seq++;
            packet.Read(this.Socket);
        }

        private CommandPacket ReceiveCommand()
        {
            var comPacket = new CommandPacket();
            seq = 0;
            ReceivePacket(comPacket);
            return comPacket;
        }
    }
}
