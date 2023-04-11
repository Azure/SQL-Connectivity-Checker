//  ---------------------------------------------------------------------------
//  <copyright file="LittleEndianUtilities.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Utilities
{
    using System.IO;
    using System;
using System.Reflection;
using System.Security.Cryptography;
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
            // Allocate buffer
            byte[] byteString = new byte[length];

            // Read into a byte buffer
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
            // Check if any data will be read
            if (length == 0)
            {
                // Instead of returning an empty string later we just return NULL
                return null;
            }

            // Allocate buffer
            byte[] byteString = new byte[length];

            // Read into a byte buffer
            source.Read(byteString, 0, byteString.Length);

            // Convert
            return Encoding.Unicode.GetString(byteString, 0, byteString.Length);
        }

        /// <summary>
        /// Write password string encrypted into the packet
        /// </summary>
        internal static void WritePasswordString(Stream destination, string value)
        {
            // Check if value is null
            if (string.IsNullOrEmpty(value))
            {
                // There's nothing to write
                return;
            }

            // Convert
            byte[] byteString = Encoding.Unicode.GetBytes(value);

            // Perform password decryption
            for (int i = 0; i < byteString.Length; i++)
            {
                // Swap 4 high bits with 4 low bits
                byteString[i] = (byte)(((byteString[i] & 0xf0) >> 4) | ((byteString[i] & 0xf) << 4));

                // XOR
                byteString[i] ^= 0xA5;
            }

            // Write into a the stream
            destination.Write(byteString, 0, byteString.Length);
        }

                /// <summary>
        /// Write string from into the packet
        /// </summary>
        internal static void WriteString(Stream destination, string value)
        {
            // Check if value is null
            if (string.IsNullOrEmpty(value))
            {
                // There's nothing to write
                return;
            }

            // Convert
            byte[] byteString = Encoding.Unicode.GetBytes(value);

            // Write into a the stream
            destination.Write(byteString, 0, byteString.Length);
        }
    }
}
