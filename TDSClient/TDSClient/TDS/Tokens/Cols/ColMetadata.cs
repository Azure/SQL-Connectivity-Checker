using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TDSClient.TDS.Interfaces;
using TDSClient.TDS.Tokens.Type;
using TDSClient.TDS.Utilities;

namespace TDSClient.TDS.Tokens.Cols
{
    public class ColMetadata : TDSToken
    {
        public uint UserType { get; set; }
        public byte Flags1 { get; set; }
        public byte Flags2 { get; set; }
        public SqlTypeInfo Type { get; set; }

        //#[Nullable]
        public string ColumnName { get; set; } = null;


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
            //TODO: IsColumnEncryptionSupported read ushort

            UserType = BigEndianUtilities.ReadUInt(stream);

            Flags1 = Convert.ToByte(stream.ReadByte());
            Flags2 = Convert.ToByte(stream.ReadByte());

            var type = new SqlTypeInfo();
            if (!type.Unpack(stream))
            {
                return false;
            }
            Type = type;

            var colNameLen = stream.ReadByte();

            var colName = BigEndianUtilities.ReadUnicodeStream(stream, colNameLen);
            ColumnName = new string(colName);

            return true;
        }
    }
}
