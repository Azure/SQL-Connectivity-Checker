//  ---------------------------------------------------------------------------
//  <copyright file="LittleEndianUtilities.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Utilities
{
using System.IO;
using System.Text;

    /// <summary>
    /// Utility class used for read and write operations on a stream containing data in little-endian byte order
    /// </summary>
    public static class LittleEndianUtilities
    {
        /// <summary>
        /// Used to write value to stream in little endian order.
        /// </summary>
        /// <param name="stream">MemoryStream to to write the value to.</param>
        /// <param name="value">Value to write to MemoryStream.</param>
        public static void WriteUShort(MemoryStream stream, ushort value)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
        }

        /// <summary>
        /// Used to write value to stream in little endian order.
        /// </summary>
        /// <param name="stream">MemoryStream to to write the value to.</param>
        /// <param name="value">Value to write to MemoryStream.</param>
        public static void WriteUInt(MemoryStream stream, uint value)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 24));
        }

        /// <summary>
        /// Used to read a UShort value from stream in little endian order.
        /// </summary>
        /// <param name="stream">MemoryStream from which to read the value.</param>
        /// <returns>UShort value read from the stream.</returns>
        public static ushort ReadUShort(MemoryStream stream)
        {
            ushort result = 0;
            for (int i = 0; i < 2; i++)
            {
                result |= (ushort)(stream.ReadByte() << (8 * i));
            }

            return result;
        }

        /// <summary>
        /// Used to read a UInt value from stream in little endian order.
        /// </summary>
        /// <param name="stream">MemoryStream from which to read the value.</param>
        /// <returns>UInt value read from the stream.</returns>
        public static uint ReadUInt(MemoryStream stream)
        {
            uint result = 0;
            for (int i = 0; i < 4; i++)
            {
                result |= (uint)(stream.ReadByte() << (8 * i));
            }
            
            return result;
        }

        /// <summary>
        /// Read a password string and decrypt it
        /// </summary>
        internal static string ReadPasswordString(Stream source, ushort length)
        {
            byte[] byteString = new byte[length];

            source.Read(byteString, 0, byteString.Length);

            // Perform password decryption
            for (int i = 0; i < byteString.Length; i++)
            {
                // XOR first
                byteString[i] ^= 0xA5;

                // Swap 4 high bits with 4 low bits
                byteString[i] = (byte)(((byteString[i] & 0xf0) >> 4) | ((byteString[i] & 0xf) << 4));
            }

            // Convert
            return Encoding.Unicode.GetString(byteString, 0, byteString.Length);
        }

        /// <summary>
        /// Read string from the packet
        /// </summary>
        internal static string ReadString(Stream source, ushort length)
        {
            if (length == 0)
            {
                return null;
            }

            byte[] byteString = new byte[length];

            source.Read(byteString, 0, byteString.Length);

            return Encoding.Unicode.GetString(byteString, 0, byteString.Length);
        }

        /// <summary>
        /// Write password string encrypted into the packet
        /// </summary>
        internal static void WritePasswordString(Stream destination, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            byte[] byteString = Encoding.Unicode.GetBytes(value);

            // Perform password decryption
            for (int i = 0; i < byteString.Length; i++)
            {
                // Swap 4 high bits with 4 low bits
                byteString[i] = (byte)(((byteString[i] & 0xf0) >> 4) | ((byteString[i] & 0xf) << 4));

                // XOR
                byteString[i] ^= 0xA5;
            }

            destination.Write(byteString, 0, byteString.Length);
        }

        /// <summary>
        /// Write string from into the packet
        /// </summary>
        internal static void WriteString(Stream destination, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            byte[] byteString = Encoding.Unicode.GetBytes(value);

            destination.Write(byteString, 0, byteString.Length);
        }

        /// <summary>
        /// Read signed integer from the packet
        /// </summary>
        internal static int ReadInt(Stream source)
        {
            return source.ReadByte()
                + source.ReadByte() << 8
                + source.ReadByte() << 16
                + source.ReadByte() << 24;
        }

        /// <summary>
        /// Write signed integer into the stream
        /// </summary>
        internal static void WriteInt(Stream destination, int value)
        {
            destination.WriteByte((byte)value);
            destination.WriteByte((byte)(value >> 8));
            destination.WriteByte((byte)(value >> 16));
            destination.WriteByte((byte)(value >> 24));
        }

        /// <summary>
        /// Read unsigned long from the stream
        /// </summary>
        internal static ulong ReadULong(Stream source)
        {
            return (ulong)(source.ReadByte()
                + (source.ReadByte() << 8)
                + (source.ReadByte() << 16)
                + (source.ReadByte() << 24)
                + (source.ReadByte() << 32)
                + (source.ReadByte() << 40)
                + (source.ReadByte() << 48)
                + (source.ReadByte() << 56));
        }

        /// <summary>
        /// Write unsigned long into the stream
        /// </summary>
        internal static void WriteULong(Stream destination, ulong value)
        {
            destination.WriteByte((byte)(value & 0xff));
            destination.WriteByte((byte)((value >> 8) & 0xff));
            destination.WriteByte((byte)((value >> 16) & 0xff));
            destination.WriteByte((byte)((value >> 24) & 0xff));
            destination.WriteByte((byte)((value >> 32) & 0xff));
            destination.WriteByte((byte)((value >> 40) & 0xff));
            destination.WriteByte((byte)((value >> 48) & 0xff));
            destination.WriteByte((byte)((value >> 56) & 0xff));
        }
    }
}
