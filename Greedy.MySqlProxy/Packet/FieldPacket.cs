using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.MySqlProxy.Packet
{
    class FieldPacket : MySqlPacket
    {
        public string Catelog { get; set; }

        public string Database { get; set; }

        public string TableAlias { get; set; }

        public string Table { get; set; }

        public string ColumnAlias { get; set; }

        public string Column { get; set; }

        public byte Filler { get { return 0x00; } }

        public CharacterSet CharacterSet { get; set; }

        public int ColumnLength { get; set; }

        public ColumnType ColumnType { get; set; }

        public ColumnFlags Flag { get; set; }

        public byte Decimal { get; set; }

        public short Filler2 { get { return 0x0000; } }

        public string DefaultValue { get; set; }

        protected override void ParseBody()
        {
            throw new NotImplementedException();
        }

        protected override void BuildBody()
        {
            throw new NotImplementedException();
        }
    }


    enum ColumnType : byte
    {
        FIELD_TYPE_DECIMAL = 0x00,
        FIELD_TYPE_TINY = 0x01,
        FIELD_TYPE_SHORT = 0x02,
        FIELD_TYPE_LONG = 0x03,
        FIELD_TYPE_FLOAT = 0x04,
        FIELD_TYPE_DOUBLE = 0x05,
        FIELD_TYPE_NULL = 0x06,
        FIELD_TYPE_TIMESTAMP = 0x07,
        FIELD_TYPE_LONGLONG = 0x08,
        FIELD_TYPE_INT24 = 0x09,
        FIELD_TYPE_DATE = 0x0A,
        FIELD_TYPE_TIME = 0x0B,
        FIELD_TYPE_DATETIME = 0x0C,
        FIELD_TYPE_YEAR = 0x0D,
        FIELD_TYPE_NEWDATE = 0x0E,
        FIELD_TYPE_VARCHAR = 0x0F,
        FIELD_TYPE_BIT = 0x10,
        FIELD_TYPE_NEWDECIMAL = 0xF6,
        FIELD_TYPE_ENUM = 0xF7,
        FIELD_TYPE_SET = 0xF8,
        FIELD_TYPE_TINY_BLOB = 0xF9,
        FIELD_TYPE_MEDIUM_BLOB = 0xFA,
        FIELD_TYPE_LONG_BLOB = 0xFB,
        FIELD_TYPE_BLOB = 0xFC,
        FIELD_TYPE_VAR_STRING = 0xFD,
        FIELD_TYPE_STRING = 0xFE,
        FIELD_TYPE_GEOMETRY = 0xFF
    }

    enum ColumnFlags : short
    {
        NOT_NULL_FLAG = 0x0001,
        PRI_KEY_FLAG = 0x0002,
        UNIQUE_KEY_FLAG = 0x0004,
        MULTIPLE_KEY_FLAG = 0x0008,
        BLOB_FLAG = 0x0010,
        UNSIGNED_FLAG = 0x0020,
        ZEROFILL_FLAG = 0x0040,
        BINARY_FLAG = 0x0080,
        ENUM_FLAG = 0x0100,
        AUTO_INCREMENT_FLAG = 0x0200,
        TIMESTAMP_FLAG = 0x0400,
        SET_FLAG = 0x0800
    }
}
