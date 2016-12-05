using System;
using System.IO;
using System.Net.Sockets;

namespace Greedy.MySqlProxy.Packet
{
    abstract class MySqlPacket : IDisposable
    {
        public int Length { get; set; }

        public byte Sequence { get; set; }

        protected Stream Body { get; set; }

        protected abstract void ParseBody();

        protected abstract void BuildBody();

        private void ReadBody(byte[] body)
        {
            this.Body = new BufferedStream(new MemoryStream(body));
        }

        private void ReadBody(Socket socket)
        {
            var buffer = new byte[1024];
            var count = 0;
            this.Body = new BufferedStream(new MemoryStream());
            while (count < this.Length)
            {
                var receivedLength = socket.Receive(buffer);
                this.Body.Write(buffer, 0, receivedLength);
                count += receivedLength;
            }
        }

        private void ReadHead(byte[] head)
        {
            if (head.Length < 4)
            {
                throw new ArgumentException("参数长度不符合格式", "data");
            }
            this.Length = (int)head[0] + (((int)head[1]) << 8) + (((int)head[2]) << 16);
            this.Sequence = head[3];
        }

        public void Read(byte[] data)
        {
            if (data == null || data.Length == 0) return;

            using (var buffer = new BufferedStream(new MemoryStream(data)))
            {
                var head = new byte[4];
                buffer.Read(head, 0, head.Length);
                ReadHead(head);

                if (this.Length > 0)
                {
                    var body = new byte[this.Length];
                    buffer.Read(body, 0, body.Length);
                    ReadBody(body);
                    ParseBody();
                }
            }
        }

        public void Read(Socket socket)
        {
            var buffer = new byte[4];
            var receivedLength = socket.Receive(buffer);
            ReadHead(buffer);

            if (this.Length > 0)
            {
                ReadBody(socket);
                ParseBody();
            }
        }

        private void WriteHead(Socket socket)
        {
            this.Length = Convert.ToInt32(this.Body.Length);
            socket.Send(BitConverter.GetBytes(this.Length), 3, SocketFlags.None);
            socket.Send(new[] { Sequence });
        }

        private void WriteBody(Socket socket)
        {
            this.Body.Position = 0;
            var buffer = new byte[1024];
            var len = 0;
            while ((len = this.Body.Read(buffer, 0, buffer.Length)) > 0)
            {
                socket.Send(buffer, len, SocketFlags.None);
            }
        }

        public void Write(Socket socket)
        {
            BuildBody();
            WriteHead(socket);
            WriteBody(socket);
        }

        public void Dispose()
        {
            this.Body.Dispose();
        }
    }

    abstract class ResultPacket : MySqlPacket
    {
        public abstract byte Header { get; }
    }
}
