using System;
using System.IO;
using Greedy.MySqlProxy.Util;

namespace Greedy.MySqlProxy.Packet
{
    class HandShakePacket : MySqlPacket
    {
        public byte ProtocolVersion { get { return 0x0a; } }

        public string ServerVersion { get; set; }

        public int ConnectionId { get; set; }

        public byte[] AuthPluginDataPart1 { get; set; }

        public byte Filler { get { return 0x00; } }

        public CapabilityFlags Capabilities { get; set; }

        //public short LowerCapabilities { get; set; }

        public CharacterSet CharacterSet { get; set; }

        public StatusFlags Status { get; set; }

        //public short UpperCapabilities { get; set; }

        public byte AuthPluginDataLength { get; set; }

        public byte[] Reserved { get { return new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; } }

        public byte[] AuthPluginDataPart2 { get; set; }

        public string AuthPluginName { get; set; }

        protected override void BuildBody()
        {
            this.Body = new BufferedStream(new MemoryStream());
            this.Body.WriteByte(ProtocolVersion);
            this.Body.WriteString(ServerVersion, true);
            this.Body.WriteInt(ConnectionId);
            this.Body.Write(AuthPluginDataPart1, 0, AuthPluginDataPart1.Length);
            this.Body.WriteByte(Filler);
            var byteCapbilities = BitConverter.GetBytes((int)Capabilities);
            this.Body.Write(byteCapbilities, 0, 2);
            this.Body.WriteByte((byte)CharacterSet);
            this.Body.Write(BitConverter.GetBytes((short)Status), 0, 2);
            this.Body.Write(byteCapbilities, 2, 2);

            if ((Capabilities & CapabilityFlags.CLIENT_PLUGIN_AUTH) > 0)
            {
                this.Body.WriteByte(AuthPluginDataLength);
            }
            else
            {
                this.Body.WriteByte(0x00);
            }

            this.Body.Write(Reserved, 0, Reserved.Length);

            if ((Capabilities & CapabilityFlags.CLIENT_SECURE_CONNECTION) > 0)
            {
                this.Body.Write(AuthPluginDataPart2, 0, AuthPluginDataPart2.Length);
            }

            if ((Capabilities & CapabilityFlags.CLIENT_PLUGIN_AUTH) > 0)
            {
                this.Body.WriteString(AuthPluginName, true);
            }
        }

        protected override void ParseBody()
        {
            this.Body.Position = 0;
            var protocolVersion = this.Body.ReadByte();
            this.ServerVersion = this.Body.ReadNulTerminatedString();
            this.ConnectionId = this.Body.ReadInt();
            this.AuthPluginDataPart1 = this.Body.ReadFixedBytes(8);
            this.Body.Position += 1;
            this.Capabilities = (CapabilityFlags)this.Body.ReadShort();
            this.CharacterSet = (CharacterSet)this.Body.ReadByte();
            this.Status = (StatusFlags)this.Body.ReadShort();
            this.Capabilities |= (CapabilityFlags)(this.Body.ReadShort() << 16);
            this.AuthPluginDataLength = (byte)this.Body.ReadByte();
            this.Body.Position += 10;

            if ((Capabilities & CapabilityFlags.CLIENT_SECURE_CONNECTION) > 0)
            {
                var length = Math.Max(13, this.AuthPluginDataLength - 8) - 1;
                this.AuthPluginDataPart2 = this.Body.ReadFixedBytes(length);
                this.Body.Position += 1;
            }

            if ((Capabilities & CapabilityFlags.CLIENT_PLUGIN_AUTH) > 0)
            {
                this.AuthPluginName = this.Body.ReadNulTerminatedString();
            }
        }
    }

    [Flags]
    enum CapabilityFlags : int
    {
        CLIENT_LONG_PASSWORD = 0x01,
        CLIENT_FOUND_ROWS = 0x02,
        CLIENT_LONG_FLAG = 0x04,
        CLIENT_CONNECT_WITH_DB = 0x08,
        CLIENT_NO_SCHEMA = 0x10,
        CLIENT_COMPRESS = 0x20,
        CLIENT_ODBC = 0x40,
        CLIENT_LOCAL_FILES = 0x80,
        CLIENT_IGNORE_SPACE = 0x100,
        CLIENT_PROTOCOL_41 = 0x200,
        CLIENT_INTERACTIVE = 0x400,
        CLIENT_SSL = 0x800,
        CLIENT_IGNORE_SIGPIPE = 0x1000,
        CLIENT_TRANSACTIONS = 0x2000,
        CLIENT_RESERVED = 0x4000,
        CLIENT_SECURE_CONNECTION = 0x8000,
        CLIENT_MULTI_STATEMENTS = 0x10000,
        CLIENT_MULTI_RESULTS = 0x20000,
        CLIENT_PS_MULTI_RESULTS = 0x40000,
        CLIENT_PLUGIN_AUTH = 0x80000,
        CLIENT_CONNECT_ATTRS = 0x100000,
        CLIENT_PLUGIN_AUTH_LENENC_CLIENT_DATA = 0x200000,
        CLIENT_CAN_HANDLE_EXPIRED_PASSWORDS = 0x400000,
        CLIENT_SESSION_TRACK = 0x800000,
        CLIENT_DEPRECATE_EOF = 0x1000000,
    }

    enum CharacterSet : byte
    {
        BIG5_CHINESE_CI = 0x01,
        LATIN1_SWEDISH_CI = 0x08,
        LATIN2_GENERAL_CI = 0x09,
        UTF8_GENERAL_CI = 0x21,
        BINARY = 0x3f
    }

    enum StatusFlags : short
    {
        SERVER_STATUS_IN_TRANS = 0x01,
        SERVER_STATUS_AUTOCOMMIT = 0x02,
        SERVER_MORE_RESULTS_EXISTS = 0x08,
        SERVER_STATUS_NO_GOOD_INDEX_USED = 0x10,
        SERVER_STATUS_NO_INDEX_USED = 0x20,
        SERVER_STATUS_CURSOR_EXISTS = 0x40,
        SERVER_STATUS_LAST_ROW_SENT = 0x80,
        SERVER_STATUS_DB_DROPPED = 0x100,
        SERVER_STATUS_NO_BACKSLASH_ESCAPES = 0x200,
        SERVER_STATUS_METADATA_CHANGED = 0x400,
        SERVER_QUERY_WAS_SLOW = 0x800,
        SERVER_PS_OUT_PARAMS = 0x1000,
        SERVER_STATUS_IN_TRANS_READONLY = 0x2000,
        SERVER_SESSION_STATE_CHANGED = 0x4000
    }
}
