//  ---------------------------------------------------------------------------
//  <copyright file="TDSPacketHeader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Header
{
    using System;
    using System.IO;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Utilities;

    public class TDSPacketHeader : IPackageable
    {
        public TDSPacketHeader() 
        { 
        }

        public TDSPacketHeader(TDSMessageType type, TDSMessageStatus status, ushort spid = 0x0000, byte packet = 0x00, byte window = 0x00)
        {
            this.Type = type;
            this.Status = status;
            this.SPID = spid;
            this.Packet = packet;
            this.Window = window;
        }

        public TDSMessageType Type { get; set; }

        public TDSMessageStatus Status { get; set; }

        public ushort Length { get; set; }

        public ushort SPID { get; set; }

        public byte Packet { get; set; }

        public byte Window { get; set; }

        public int ConvertedPacketLength
        {
            get
            {
                return Convert.ToInt32(this.Length);
            }
        }

        public void Pack(MemoryStream stream)
        {
            stream.WriteByte((byte)this.Type);
            stream.WriteByte((byte)this.Status);
            BigEndianUtilities.WriteUShort(stream, this.Length);
            BigEndianUtilities.WriteUShort(stream, this.SPID);
            stream.WriteByte(this.Packet);
            stream.WriteByte(this.Window);
        }

        public bool Unpack(MemoryStream stream)
        {
            this.Type = (TDSMessageType)stream.ReadByte();
            this.Status = (TDSMessageStatus)stream.ReadByte();
            this.Length = BigEndianUtilities.ReadUShort(stream);
            this.SPID = BigEndianUtilities.ReadUShort(stream);
            this.Packet = Convert.ToByte(stream.ReadByte());
            this.Window = Convert.ToByte(stream.ReadByte());
            
            return true;
        }
    }
}
