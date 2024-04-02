//  ---------------------------------------------------------------------------
//  <copyright file="LittleEndianUtilities.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Utilities
{
    using System.IO;

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
    }
}
