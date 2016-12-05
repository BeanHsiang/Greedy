using Greedy.MySqlProxy.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace Greedy.MySqlProxy.Packet
{
    class HandshakeResponse41Packet : MySqlPacket
    {
        public CapabilityFlags Capabilities { get; set; }

        public int MaxPacketSize { get; set; }

        public CharacterSet CharacterSet { get; set; }

        public byte[] Reserved { get { return new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; } }

        public string UserName { get; set; }

        public byte[] AuthResponse { get; set; }

        public string Database { get; set; }

        public string AuthPluginName { get; set; }

        public IDictionary<string, string> ConnectionAttributes { get; set; }

        protected override void ParseBody()
        {
            this.Body.Position = 0;
            this.Capabilities = (CapabilityFlags)this.Body.ReadInt();
            this.MaxPacketSize = this.Body.ReadInt();
            this.CharacterSet = (CharacterSet)this.Body.ReadByte();
            this.Body.Position += 23;
            this.UserName = this.Body.ReadNulTerminatedString();

            if ((this.Capabilities & CapabilityFlags.CLIENT_PLUGIN_AUTH_LENENC_CLIENT_DATA) > 0)
            {
                var length = this.Body.ReadLengthEncodedInt();
                this.AuthResponse = new byte[length];
                this.Body.Read(this.AuthResponse, 0, this.AuthResponse.Length);
            }
            else if ((this.Capabilities & CapabilityFlags.CLIENT_SECURE_CONNECTION) > 0)
            {
                var length = this.Body.ReadByte();
                this.AuthResponse = new byte[length];
                this.Body.Read(this.AuthResponse, 0, this.AuthResponse.Length);
            }
            else
            {
                this.AuthResponse = this.Body.ReadNulTerminatedBytes();
            }

            if ((this.Capabilities & CapabilityFlags.CLIENT_CONNECT_WITH_DB) > 0)
            {
                this.Database = this.Body.ReadNulTerminatedString();
            }

            if ((this.Capabilities & CapabilityFlags.CLIENT_PLUGIN_AUTH) > 0)
            {
                this.AuthPluginName = this.Body.ReadNulTerminatedString();
            }

            if ((this.Capabilities & CapabilityFlags.CLIENT_CONNECT_ATTRS) > 0)
            {
                var attributesCount = this.Body.ReadLengthEncodedInt();
                this.ConnectionAttributes = new Dictionary<string, string>();
                for (int i = 0; i < attributesCount; i++)
                {
                    var key = this.Body.ReadLengthEncodedString();
                    var value = this.Body.ReadLengthEncodedString();
                    this.ConnectionAttributes.Add(key, value);
                }
            }
        }

        protected override void BuildBody()
        {
            this.Body = new BufferedStream(new MemoryStream());
            this.Body.WriteInt((int)this.Capabilities);
            this.Body.WriteInt(this.MaxPacketSize);
            this.Body.WriteByte((byte)this.CharacterSet);
            this.Body.Write(this.Reserved, 0, this.Reserved.Length);
            this.Body.WriteString(this.UserName, true);

            if ((this.Capabilities & CapabilityFlags.CLIENT_PLUGIN_AUTH_LENENC_CLIENT_DATA) > 0)
            {
                this.Body.WriteLengthEncodedInt(this.AuthResponse.Length);
                this.Body.Write(this.AuthResponse, 0, this.AuthResponse.Length);
            }
            else if ((this.Capabilities & CapabilityFlags.CLIENT_SECURE_CONNECTION) > 0)
            {
                this.Body.WriteByte((byte)this.AuthResponse.Length);
                this.Body.Write(this.AuthResponse, 0, this.AuthResponse.Length);
            }
            else
            {
                this.Body.Write(this.AuthResponse, 0, this.AuthResponse.Length);
                this.Body.WriteByte(0x00);
            }

            if ((this.Capabilities & CapabilityFlags.CLIENT_CONNECT_WITH_DB) > 0)
            {
                this.Body.WriteString(this.Database, true);
            }

            if ((this.Capabilities & CapabilityFlags.CLIENT_PLUGIN_AUTH) > 0)
            {
                this.Body.WriteString(this.AuthPluginName, true);
            }

            if ((this.Capabilities & CapabilityFlags.CLIENT_CONNECT_ATTRS) > 0)
            {
                this.Body.WriteLengthEncodedInt(ConnectionAttributes.Count);
                foreach (var attr in ConnectionAttributes)
                {
                    this.Body.WriteLengthEncodedString(attr.Key);
                    this.Body.WriteLengthEncodedString(attr.Value);
                }
            }
        }
    }
}
