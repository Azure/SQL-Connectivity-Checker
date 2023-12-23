using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TDSClient.TDS.Utilities;

namespace TDSClient.TDS.Tokens.Type
{
    public static class SqlTypeValueFactory
    {
        public static object ReadVaue(MemoryStream stream, SqlTypeInfo type)
        {
            if (type.IsNullLen)
            {
                return null;
            }

            if(type.IsFixedLen)
            {
                return ReadFixedTypeValue(stream, type);
            }

            throw new NotSupportedException();
        }

        private static object ReadFixedTypeValue(MemoryStream stream, SqlTypeInfo type)
        {
            byte[] buffer;
            switch (type.Type)
            {
                case SqlTypes.INT1TYPE:
                    return (sbyte)stream.ReadByte();

                case SqlTypes.BITTYPE:
                    return (byte)stream.ReadByte();

                case SqlTypes.INT2TYPE:
                    return (short)BigEndianUtilities.ReadUShortLE(stream);

                case SqlTypes.INT4TYPE:
                    return (int)BigEndianUtilities.ReadUIntLE(stream);

                case SqlTypes.DATETIM4TYPE:
                    buffer = new byte[4];
                    stream.Read(buffer, 0, 4);
                    return ReadSmalldatetimeFromBytes(buffer);

                case SqlTypes.FLT4TYPE:
                    buffer = new byte[4];
                    stream.Read(buffer, 0, 4);
                    return BitConverter.ToSingle(buffer, 0);

                case SqlTypes.MONEY4TYPE:
                    buffer = new byte[4];
                    stream.Read(buffer, 0, 4);
                    return ReadMoney4FromBytes(buffer);

                case SqlTypes.DATETIMETYPE:
                    buffer = new byte[8];
                    stream.Read(buffer, 0, 8);
                    return ReadDatetimeFromBytes(buffer);

                case SqlTypes.FLT8TYPE:
                    buffer = new byte[8];
                    stream.Read(buffer, 0, 8);
                    return BitConverter.ToDouble(buffer, 0);

                case SqlTypes.INT8TYPE:
                    return (long)BigEndianUtilities.ReadULongLE(stream);

                case SqlTypes.MONEYTYPE:
                    buffer = new byte[8];
                    stream.Read(buffer, 0, 8);
                    return ReadMoneyFromBytes(buffer);

                default:
                    throw new ArgumentException("Unknown fixed length type");
            }
        }

        private static DateTime ReadSmalldatetimeFromBytes(byte[] bytes)
        {
            if (bytes.Length != 4)
                throw new ArgumentException("Invalid byte array length.");

            // First two bytes represent days since 1900-01-01
            short days = BitConverter.ToInt16(bytes, 0);

            // Next two bytes represent minutes since midnight
            short minutes = BitConverter.ToInt16(bytes, 2);

            DateTime baseDate = new DateTime(1900, 1, 1);
            return baseDate.AddDays(days).AddMinutes(minutes);
        }

        private static decimal ReadMoneyFromBytes(byte[] bytes)
        {
            if (bytes.Length != 8)
                throw new ArgumentException("Invalid byte array length for MONEYTYPE.");

            long value = BitConverter.ToInt64(bytes, 0);
            return value / 10000m; // scaled by 10,000 for 4-digit precision
        }

        private static decimal ReadMoney4FromBytes(byte[] bytes)
        {
            if (bytes.Length != 4)
                throw new ArgumentException("Invalid byte array length for MONEY4TYPE.");

            int value = BitConverter.ToInt32(bytes, 0);
            return value / 10000m; // scaled by 10,000 for 4-digit precision
        }

        private static DateTime ReadDatetimeFromBytes(byte[] bytes)
        {
            if (bytes.Length != 8)
                throw new ArgumentException("Invalid byte array length for DATETIMETYPE.");

            int days = BitConverter.ToInt32(bytes, 0);
            int timeUnits = BitConverter.ToInt32(bytes, 4); // number of 300th of a second units

            DateTime baseDate = new DateTime(1900, 1, 1);
            return baseDate.AddDays(days).AddMilliseconds(timeUnits * 3.333333); // Convert 300th of a second units to milliseconds
        }
    }
}
