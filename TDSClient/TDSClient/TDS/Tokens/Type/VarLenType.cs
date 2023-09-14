using System;
using System.Collections.Generic;
using System.Text;

namespace TDSClient.TDS.Tokens.Type
{
    [Flags]
    public enum VarLenType: byte
    {
        //Zero length data types
        NULLTYPE = 0x1F,

        //Fixed lenght data types
        INT1TYPE = 0x30,
        BITTYPE = 0x32,
        INT2TYPE = 0x34,
        INT4TYPE = 0x38,
        DATETIM4TYPE = 0x3A,
        FLT4TYPE = 0x3B,
        MONEYTYPE = 0x3C,
        DATETIMETYPE = 0x3D,
        FLT8TYPE = 0x3E,
        MONEY4TYPE = 0x7A,
        INT8TYPE = 0x7F,
        DECIMALTYPE = 0x37,
        NUMERICTYPE = 0x3F,

        FIXEDLENTYPE = INT1TYPE
                     | BITTYPE
                     | INT2TYPE
                     | INT4TYPE
                     | DATETIM4TYPE
                     | FLT4TYPE
                     | MONEYTYPE
                     | DATETIMETYPE
                     | FLT8TYPE
                     | MONEY4TYPE
                     | INT8TYPE,

        //Variable length data types
        GUIDTYPE = 0x24,
        INTNTYPE = 0x26,
        BITNTYPE = 0x68,
        DECIMALNTYPE = 0x6A,
        NUMERICNTYPE = 0x6C,
        FLTNTYPE = 0x6D,
        MONEYNTYPE = 0x6E,
        DATETIMNTYPE = 0x6F,
        DATENTYPE = 0x28,
        TIMENTYPE = 0x29,
        DATETIME2NTYPE = 0x2A,
        DATETIMEOFFSETNTYPE = 0x2B,
        CHARTYPE = 0x2F,
        VARCHARTYPE = 0x27,
        BINARYTYPE = 0x2D,
        VARBINARYTYPE = 0x25,
        BIGVARBINARYTYPE = 0xA5,
        BIGVARCHARTYPE = 0xA7,
        BIGBINARYTYPE = 0xAD,
        BIGCHARTYPE = 0xAF,
        NVARCHARTYPE = 0xE7,
        NCHARTYPE = 0xEF,
        XMLTYPE = 0xF1,
        UDTTYPE = 0xF0,
        TEXTTYPE = 0x23,
        IMAGETYPE = 0x22,
        NTEXTTYPE = 0x63,
        SSVARIANTTYPE = 0x62,

        BYTELEN_TYPE = GUIDTYPE
                        | INTNTYPE
                        | DECIMALTYPE
                        | NUMERICTYPE
                        | BITNTYPE
                        | DECIMALNTYPE
                        | NUMERICNTYPE
                        | FLTNTYPE
                        | MONEYNTYPE
                        | DATETIMNTYPE
                        | DATENTYPE
                        | TIMENTYPE
                        | DATETIME2NTYPE
                        | DATETIMEOFFSETNTYPE
                        | CHARTYPE
                        | VARCHARTYPE
                        | BINARYTYPE
                        | VARBINARYTYPE,

        USHORTLEN_TYPE = BIGVARBINARYTYPE
                         | BIGVARCHARTYPE
                         | BIGBINARYTYPE
                         | BIGCHARTYPE
                         | NVARCHARTYPE
                         | NCHARTYPE,

        LONGLEN_TYPE = IMAGETYPE
                         | NTEXTTYPE
                         | SSVARIANTTYPE
                         | TEXTTYPE
                         | XMLTYPE,

        VARLENTYPE = BYTELEN_TYPE
                         | USHORTLEN_TYPE
                         | LONGLEN_TYPE,

        //Partially length type
        PARTLENTYPE = XMLTYPE
                         | BIGVARCHARTYPE
                         | BIGVARBINARYTYPE
                         | NVARCHARTYPE
                         | UDTTYPE,

    }
}
