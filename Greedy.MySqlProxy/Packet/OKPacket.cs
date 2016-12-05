using System;
using System.Collections.Generic;
using System.IO;
using Greedy.MySqlProxy.Util;

namespace Greedy.MySqlProxy.Packet
{
    class OKPacket : ResultPacket
    {
        public override byte Header { get { return 0x00; } }

        public int AffectedRows { get; set; }

        public int LastInsertId { get; set; }

        public CapabilityFlags Capabilities { get; set; }

        public StatusFlags Status { get; set; }

        public short WarningsNumber { get; set; }

        public string Info { get; set; }

        public SessionStateInformation SessionStateInfo { get; set; }

        protected override void ParseBody()
        {
            this.Body.Seek(1, SeekOrigin.Begin);
            this.AffectedRows = (int)this.Body.ReadLengthEncodedInt();
            this.LastInsertId = (int)this.Body.ReadLengthEncodedInt();

            if ((Capabilities & CapabilityFlags.CLIENT_PROTOCOL_41) > 0)
            {
                this.Status = (StatusFlags)this.Body.ReadShort();
                this.WarningsNumber = this.Body.ReadShort();
            }
            else if ((Capabilities & CapabilityFlags.CLIENT_TRANSACTIONS) > 0)
            {
                this.Status = (StatusFlags)this.Body.ReadShort();
            }

            if ((Capabilities & CapabilityFlags.CLIENT_SESSION_TRACK) > 0)
            {
                this.Info = this.Body.ReadLengthEncodedString();
                if ((Status & StatusFlags.SERVER_SESSION_STATE_CHANGED) > 0)
                {
                    SessionStateInfo.Read(this.Body);
                }
            }
            else
            {
                this.Info = this.Body.ReadNulTerminatedString();
            }
        }

        protected override void BuildBody()
        {
            this.Body = new BufferedStream(new MemoryStream());
            this.Body.WriteByte(Header);
            this.Body.WriteLengthEncodedInt(AffectedRows);
            this.Body.WriteLengthEncodedInt(LastInsertId);

            if ((Capabilities & CapabilityFlags.CLIENT_PROTOCOL_41) > 0)
            {
                this.Body.WriteShort((short)Status);
                this.Body.WriteShort(WarningsNumber);
            }
            else if ((Capabilities & CapabilityFlags.CLIENT_TRANSACTIONS) > 0)
            {
                this.Body.WriteShort((short)Status);
            }

            if ((Capabilities & CapabilityFlags.CLIENT_SESSION_TRACK) > 0)
            {
                this.Body.WriteLengthEncodedString(Info);
                if ((Status & StatusFlags.SERVER_SESSION_STATE_CHANGED) > 0)
                {
                    SessionStateInfo.Write(this.Body);
                }
            }
            else
            {
                this.Body.WriteString(Info);
            }
        }
    }

    class SessionStateInformation
    {
        public StateChangeType StateChangeType { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public bool IsTracked { get; set; }

        public void Write(Stream stream)
        {
            var data = new List<byte>();
            if (StateChangeType == StateChangeType.SESSION_TRACK_STATE_CHANGE)
            {
                data.Add(0x01);
                if (IsTracked)
                {
                    data.Add(0x31);
                }
                else
                {
                    data.Add(0x30);
                }
            }
            else if (StateChangeType == StateChangeType.SESSION_TRACK_SCHEMA)
            {
                data.AddRange(DataType.GetLengthEncodedString(Name));
            }
            else if (StateChangeType == StateChangeType.SESSION_TRACK_SYSTEM_VARIABLES)
            {
                data.AddRange(DataType.GetLengthEncodedString(Name));
                data.AddRange(DataType.GetLengthEncodedString(Value));
            }

            var length = data.Count;
            data.InsertRange(0, DataType.GetLengthEncodedInt(length));
            data.Insert(0, (byte)StateChangeType);

            length = data.Count;
            data.InsertRange(0, DataType.GetLengthEncodedInt(length));

            var byts = data.ToArray();
            stream.Write(byts, 0, byts.Length);
        }

        public void Read(Stream stream)
        {
            var length = stream.ReadLengthEncodedInt();
            this.StateChangeType = (StateChangeType)stream.ReadByte();
            var dataLength = stream.ReadLengthEncodedInt();
            if (StateChangeType == Packet.StateChangeType.SESSION_TRACK_STATE_CHANGE)
            {
                var value = stream.ReadLengthEncodedInt();
                this.IsTracked = value == 0x31;
            }
            else if (StateChangeType == StateChangeType.SESSION_TRACK_SCHEMA)
            {
                Name = stream.ReadLengthEncodedString();
            }
            else if (StateChangeType == StateChangeType.SESSION_TRACK_SYSTEM_VARIABLES)
            {
                Name = stream.ReadLengthEncodedString();
                Value = stream.ReadLengthEncodedString();
            }
        }
    }

    enum StateChangeType : byte
    {
        SESSION_TRACK_SYSTEM_VARIABLES = 0x00,
        SESSION_TRACK_SCHEMA = 0x01,
        SESSION_TRACK_STATE_CHANGE = 0x02,
        SESSION_TRACK_GTIDS = 0x03
    }
}
