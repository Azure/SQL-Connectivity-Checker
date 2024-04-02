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

    /// <summary>
    /// Class describing TDS Packet Header
    /// </summary>
    public class TDSPacketHeader : IPackageable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TDSPacketHeader" /> class.
        /// </summary>
        public TDSPacketHeader() 
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TDSPacketHeader" /> class.
        /// </summary>
        /// <param name="type">TDS Message Type</param>
        /// <param name="status">TDS Message Status</param>
        /// <param name="spid">SPID number</param>
        /// <param name="packet">Packet number</param>
        /// <param name="window">Window number</param>
        public TDSPacketHeader(TDSMessageType type, TDSMessageStatus status, ushort spid = 0x0000, byte packet = 0x00, byte window = 0x00)
        {
            this.Type = type;
            this.Status = status;
            this.SPID = spid;
            this.Packet = packet;
            this.Window = window;
        }

        /// <summary>
        /// Gets or sets TDS Message Type.
        /// </summary>
        public TDSMessageType Type { get; set; }

        /// <summary>
        /// Gets or sets TDS Message Status.
        /// </summary>
        public TDSMessageStatus Status { get; set; }

        /// <summary>
        /// Gets or sets TDS Message Length.
        /// </summary>
        public ushort Length { get; set; }

        /// <summary>
        /// Gets or sets SPID.
        /// </summary>
        public ushort SPID { get; set; }

        /// <summary>
        /// Gets or sets Packet Number.
        /// </summary>
        public byte Packet { get; set; }

        /// <summary>
        /// Gets or sets Window Number.
        /// </summary>
        public byte Window { get; set; }

        /// <summary>
        /// Gets converted (to int) Packet Length.
        /// </summary>
        public int ConvertedPacketLength
        {
            get
            {
                return Convert.ToInt32(this.Length);
            }
        }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public void Pack(MemoryStream stream)
        {
            stream.WriteByte((byte)this.Type);
            stream.WriteByte((byte)this.Status);
            BigEndianUtilities.WriteUShort(stream, this.Length);
            BigEndianUtilities.WriteUShort(stream, this.SPID);
            stream.WriteByte(this.Packet);
            stream.WriteByte(this.Window);
        }

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
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
