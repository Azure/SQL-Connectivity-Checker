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

    static public class BigEndianUtilities
    {
        public static void WriteUShort(MemoryStream stream, ushort value)
        {
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        public static void WriteUInt(MemoryStream stream, uint value)
        {
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

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

        public static void WriteByteArray(MemoryStream stream, byte[] array)
        {
            for (int i = array.Length - 1; i >= 0; i--)
            {
                stream.WriteByte(array[i]);
            }
        }

        public static ushort ReadUShort(MemoryStream stream)
        {
            ushort result = 0;
            for (int i = 0; i < 2; i++)
            {
                result <<= 8;
                result |= (byte)stream.ReadByte();
            }
            return result;
        }

        public static uint ReadUInt(MemoryStream stream)
        {
            uint result = 0;
            for (int i = 0; i < 4; i++)
            {
                result <<= 8;
                result |= (byte)stream.ReadByte();
            }
            return result;
        }

        public static ulong ReadULong(MemoryStream stream)
        {
            ulong result = 0;
            for (int i = 0; i < 8; i++)
            {
                result <<= 8;
                result |= (byte)stream.ReadByte();
            }
            return result;
        }

        public static byte[] ReadByteArray(MemoryStream stream, uint length)
        {
            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                result[length - i] = (byte)stream.ReadByte();
            }
            return result;
        }
    }
}
