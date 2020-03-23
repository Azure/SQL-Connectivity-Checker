//  ---------------------------------------------------------------------------
//  <copyright file="BigEndianUtilities.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;

    public static class BigEndianUtilities
    {
        /// <summary>
        /// Used to write value to stream in big endian order.
        /// </summary>
        /// <param name="stream">MemoryStream to to write the value to.</param>
        /// <param name="value">Value to write to MemoryStream.</param>
        public static void WriteUShort(MemoryStream stream, ushort value)
        {
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        /// <summary>
        /// Used to write value to stream in big endian order.
        /// </summary>
        /// <param name="stream">MemoryStream to to write the value to.</param>
        /// <param name="value">Value to write to MemoryStream.</param>
        public static void WriteUInt(MemoryStream stream, uint value)
        {
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        /// <summary>
        /// Used to write value to stream in big endian order.
        /// </summary>
        /// <param name="stream">MemoryStream to to write the value to.</param>
        /// <param name="value">Value to write to MemoryStream.</param>
        public static void WriteULong(MemoryStream stream, ulong value)
        {
            stream.WriteByte((byte)(value >> 56));
            stream.WriteByte((byte)(value >> 48));
            stream.WriteByte((byte)(value >> 40));
            stream.WriteByte((byte)(value >> 32));
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        /// <summary>
        /// Used to write byte array to stream in big endian order.
        /// </summary>
        /// <param name="stream">MemoryStream to to write the value to.</param>
        /// <param name="array">Array to write to MemoryStream.</param>
        public static void WriteByteArray(MemoryStream stream, byte[] array)
        {
            for (int i = array.Length - 1; i >= 0; i--)
            {
                stream.WriteByte(array[i]);
            }
        }

        /// <summary>
        /// Used to read a UShort value from stream in big endian order.
        /// </summary>
        /// <param name="stream">MemoryStream from which to read the value.</param>
        /// <returns>UShort value read from the stream.</returns>
        public static ushort ReadUShort(MemoryStream stream)
        {
            ushort result = 0;
            for (int i = 0; i < 2; i++)
            {
                result <<= 8;
                result |= Convert.ToByte(stream.ReadByte());
            }

            return result;
        }

        /// <summary>
        /// Used to read a UInt value from stream in big endian order.
        /// </summary>
        /// <param name="stream">MemoryStream from which to read the value.</param>
        /// <returns>UInt value read from the stream.</returns>
        public static uint ReadUInt(MemoryStream stream)
        {
            uint result = 0;
            for (int i = 0; i < 4; i++)
            {
                result <<= 8;
                result |= Convert.ToByte(stream.ReadByte());
            }

            return result;
        }

        /// <summary>
        /// Used to read a ULong value from stream in big endian order.
        /// </summary>
        /// <param name="stream">MemoryStream from which to read the value.</param>
        /// <returns>ULong value read from the stream.</returns>
        public static ulong ReadULong(MemoryStream stream)
        {
            ulong result = 0;
            for (int i = 0; i < 8; i++)
            {
                result <<= 8;
                result |= Convert.ToByte(stream.ReadByte());
            }

            return result;
        }

        /// <summary>
        /// Used to read a byte array from stream in big endian order.
        /// </summary>
        /// <param name="stream">MemoryStream from which to read the array.</param>
        /// <param name="length">Length of the array to read.</param>
        /// <returns>Byte Array read from the stream.</returns>
        public static byte[] ReadByteArray(MemoryStream stream, uint length)
        {
            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                result[length - i] = Convert.ToByte(stream.ReadByte());
            }

            return result;
        }
    }
}
