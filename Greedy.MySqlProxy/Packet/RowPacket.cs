using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Greedy.MySqlProxy.Util;

namespace Greedy.MySqlProxy.Packet
{
    class RowPacket : MySqlPacket
    {
        public string[] FieldValues { get; set; }

        public int FieldCount { get; set; }

        protected override void ParseBody()
        {
            this.Body.Position = 0;
            this.FieldValues = new string[FieldCount];
            for (int i = 0; i < FieldCount; i++)
            {
                this.FieldValues[i] = this.Body.ReadLengthEncodedString();
            }

        }

        protected override void BuildBody()
        {
            throw new NotImplementedException();
        }
    }

    class BinaryRowPacket : MySqlPacket
    {
        public byte[] Bitmap { get; set; }

        public string[] FieldValues { get; set; }

        public int FieldCount { get; set; }


        protected override void ParseBody()
        {
            this.Body.Position = 0;

        }

        protected override void BuildBody()
        {
            throw new NotImplementedException();
        }
    }

}
