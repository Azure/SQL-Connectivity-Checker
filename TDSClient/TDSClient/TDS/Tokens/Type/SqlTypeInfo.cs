using System;
using System.IO;
using TDSClient.TDS.Interfaces;

namespace TDSClient.TDS.Tokens.Type
{
    public class SqlTypeInfo : IPackageable
    {
        public SqlTypes Type { get; set; }

        public bool IsNullLen => Type == SqlTypes.NULLTYPE;

        public bool IsFixedLen => SqlTypes.FIXEDLENTYPE.HasFlag(Type);

        public bool IsVarLenType => SqlTypes.VARLENTYPE.HasFlag(Type);

        public bool IsPartLenType => SqlTypes.PARTLENTYPE.HasFlag(Type);


        public void Pack(MemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool Unpack(MemoryStream stream)
        {
            Type = (SqlTypes)stream.ReadByte();
            return true;
        }

        public int GetFixedLen()
        {
            switch (Type)
            {
                case SqlTypes.INT1TYPE:
                case SqlTypes.BITTYPE:
                    return 1;

                case SqlTypes.INT2TYPE:
                    return 2;

                case SqlTypes.INT4TYPE:
                case SqlTypes.DATETIM4TYPE:
                case SqlTypes.FLT4TYPE:
                case SqlTypes.MONEY4TYPE:
                    return 4;

                case SqlTypes.MONEYTYPE:
                case SqlTypes.DATETIMETYPE:
                case SqlTypes.FLT8TYPE:
                case SqlTypes.INT8TYPE:
                    return 8;

                default:
                    throw new ArgumentException("Unknown fixed length type");
            }
        }
    }
}
