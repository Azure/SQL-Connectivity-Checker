//  ---------------------------------------------------------------------------
//  <copyright file="LittleEndianUtilities.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Utilities
{
    using System.IO;

    public static class LittleEndianUtilities
    {
        public static void WriteUShort(MemoryStream stream, ushort value)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
        }

        public static void WriteUInt(MemoryStream stream, uint value)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 24));
        }

        public static void WriteULong(MemoryStream stream, ulong value)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 32));
            stream.WriteByte((byte)(value >> 40));
            stream.WriteByte((byte)(value >> 48));
            stream.WriteByte((byte)(value >> 56));
        }

        public static ushort ReadUShort(MemoryStream stream)
        {
            ushort result = 0;
            for (int i = 0; i < 2; i++)
            {
                result |= (ushort)(stream.ReadByte() << (8 * i));
            }

            return result;
        }

        public static uint ReadUInt(MemoryStream stream)
        {
            uint result = 0;
            for (int i = 0; i < 4; i++)
            {
                result |= (uint)(stream.ReadByte() << (8 * i));
            }
            
            return result;
        }

        public static ulong ReadULong(MemoryStream stream)
        {
            ulong result = 0;
            for (int i = 0; i < 8; i++)
            {
                result |= (ulong)stream.ReadByte() << (8 * i);
            }
            
            return result;
        }
    }
}
