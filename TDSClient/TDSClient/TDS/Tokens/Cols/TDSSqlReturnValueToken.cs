using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using TDSClient.TDS.Interfaces;
using TDSClient.TDS.Tokens.Type;
using TDSClient.TDS.Utilities;

namespace TDSClient.TDS.Tokens.Cols
{
    internal class TDSSqlReturnValueToken : TDSToken
    {
        public ushort Index { get; set; }
        //[Nullable]
        public string ParameterName { get; set; } = null;
        public byte Status { get; set; }
        public uint UserType { get; set; }
        public byte Flags1 { get; set; }
        public byte Flags2 { get; set; }
        public SqlTypeInfo Type { get; set; }

        public object Value { get; set; } = null;

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override ushort Length()
        {
            throw new NotImplementedException();
        }

        public override void Pack(MemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public override bool Unpack(MemoryStream stream)
        {
            Index = BigEndianUtilities.ReadUShortLE(stream);
            var paraeterNameLen = stream.ReadByte();
            var paraeterName = BigEndianUtilities.ReadUnicodeStream(stream, paraeterNameLen);
            ParameterName = new string(paraeterName);
            Status = Convert.ToByte(stream.ReadByte());

            UserType = BigEndianUtilities.ReadUInt(stream);
            Flags1 = Convert.ToByte(stream.ReadByte());
            Flags2 = Convert.ToByte(stream.ReadByte());

            var type = new SqlTypeInfo();
            if (!type.Unpack(stream))
            {
                return false;
            }
            Type = type;


            Value = SqlTypeValueFactory.ReadVaue(stream, type);

            return true;
        }

    }
}
