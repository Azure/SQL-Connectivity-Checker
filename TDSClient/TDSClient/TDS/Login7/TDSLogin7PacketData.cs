//  ---------------------------------------------------------------------------
//  <copyright file="TDSLogin7PacketData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Login7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Utilities;

    /// <summary>
    /// Data portion of the TDS Login7 packet
    /// </summary>
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class TDSLogin7PacketData : ITDSPacketData, IEquatable<TDSLogin7PacketData>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TDSLogin7PacketData" /> class.
        /// </summary>
        public TDSLogin7PacketData()
        {
            this.TDSVersion = 1946157060; // TDS 7.4
            this.ConnectionID = 0;
            this.ClientPID = (uint)Process.GetCurrentProcess().Id;
            this.PacketSize = 4096;
            this.ClientProgVer = 117440512;
            this.OptionFlags1 = new TDSLogin7OptionFlags1();
            this.OptionFlags2 = new TDSLogin7OptionFlags2();
            this.OptionFlags3 = new TDSLogin7OptionFlags3();
            this.TypeFlags = new TDSLogin7TypeFlags();
            this.OffsetLength = new TDSLogin7OffsetLength();
        }

        /// <summary>
        /// Gets the total length of the LOGIN7 structure.
        /// </summary>
        public uint Length
        {
            get
            {
                return Convert.ToUInt32((7 * sizeof(uint)) + sizeof(int) + (4 * sizeof(byte)) + this.Data.Length + (24 * sizeof(ushort)) + sizeof(uint) + (6 * sizeof(byte)));
            }
        }

        /// <summary>
        /// Gets or sets the highest TDS version being used by the client.
        /// </summary>
        public uint TDSVersion { get; set; }

        /// <summary>
        /// Gets or sets the packet size being requested by the client.
        /// </summary>
        public uint PacketSize { get; set; }

        /// <summary>
        /// Gets or sets the version of the interface library (for example, ODBC or OLEDB) being used by the client.
        /// </summary>
        public uint ClientProgVer { get; set; }

        /// <summary>
        /// Gets or sets the process ID of the client application.
        /// </summary>
        public uint ClientPID { get; set; }

        /// <summary>
        /// Gets or sets the connection ID of the primary Server. Used when connecting to an "Always Up" backup
        /// server.
        /// </summary>
        public uint ConnectionID { get; set; }

        /// <summary>
        /// Gets or sets Option Flags 1.
        /// </summary>
        public TDSLogin7OptionFlags1 OptionFlags1 { get; set; }

        /// <summary>
        /// Gets or sets Option Flags 2.
        /// </summary>
        public TDSLogin7OptionFlags2 OptionFlags2 { get; set; }

        /// <summary>
        /// Gets or sets Type Flags.
        /// </summary>
        public TDSLogin7TypeFlags TypeFlags { get; set; }

        /// <summary>
        /// Gets or sets Option Flags 3.
        /// </summary>
        public TDSLogin7OptionFlags3 OptionFlags3 { get; set; }

        /// <summary>
        /// Gets or sets ClientTimeZone.
        /// This field is not used and can be set to zero.
        /// </summary>
        public int ClientTimeZone { get; set; }

        /// <summary>
        /// Gets or sets Client LCID.
        /// The language code identifier (LCID) value for the client collation. 
        /// If ClientLCID is specified, the specified collation is set as the 
        /// session collation.
        /// </summary>
        public uint ClientLCID { get; set; }

        /// <summary>
        /// Gets or sets the variable portion of this message. A stream of bytes in the order shown, indicates the offset
        /// (from the start of the message) and length of various parameters.
        /// </summary>
        public TDSLogin7OffsetLength OffsetLength { get; set; }

        /// <summary>
        /// Gets or sets the actual variable-length data portion referred to by OffsetLength.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Add TDS Login7 Option.
        /// </summary>
        /// <param name="optionName">Option Name</param>
        /// <param name="length">Option Length</param>
        /// <param name="data">Option Data</param>
        public void AddOption(string optionName, ushort length, object data)
        {
            if (optionName == null || data == null)
            {
                throw new ArgumentNullException();
            }

            this.OffsetLength.AddOptionPositionInfo(optionName, length);
            var prevLength = this.Data != null ? this.Data.Length : 0;

            byte[] optionData;
            if (optionName != "SSPI")
            {
                if (!(data is string))
                {
                    throw new ArgumentException();
                }

                optionData = Encoding.Unicode.GetBytes((string)data);

                if (optionName != "Password")
                {
                    LoggingUtilities.WriteLogVerboseOnly($" Adding Login7 option {optionName} [{(string)data}].");
                }
                else
                {
                    LoggingUtilities.WriteLogVerboseOnly($" Adding Login7 option {optionName}.");
                }
            }
            else
            {
                if (!(data is byte[]))
                {
                    throw new ArgumentException();
                }

                optionData = (byte[])data;
                LoggingUtilities.WriteLogVerboseOnly($" Adding Login7 option {optionName}.");
            }

            var temp = this.Data;
            Array.Resize(ref temp, prevLength + optionData.Length);
            this.Data = temp;

            if (optionName == "Password")
            {
                for (int i = 0; i < optionData.Length; i++)
                {
                    var piece0 = (byte)(optionData[i] >> 4);
                    var piece1 = (byte)(optionData[i] & 0x0f);
                    optionData[i] = (byte)((piece0 | (piece1 << 4)) ^ 0xA5);
                }
            }

            Array.Copy(optionData, 0, this.Data, prevLength, optionData.Length);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as TDSLogin7PacketData);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public bool Equals(TDSLogin7PacketData other)
        {
            return other != null &&
                   ((this.Data != null && this.Data.SequenceEqual(other.Data)) || (this.Data == other.Data)) &&
                   this.Length == other.Length &&
                   this.TDSVersion == other.TDSVersion &&
                   this.PacketSize == other.PacketSize &&
                   this.ClientProgVer == other.ClientProgVer &&
                   this.ClientPID == other.ClientPID &&
                   this.ConnectionID == other.ConnectionID &&
                   this.OptionFlags1.Equals(other.OptionFlags1) &&
                   this.OptionFlags2.Equals(other.OptionFlags2) &&
                   this.TypeFlags.Equals(other.TypeFlags) &&
                   this.OptionFlags3.Equals(other.OptionFlags3) &&
                   this.ClientTimeZone == other.ClientTimeZone &&
                   this.ClientLCID == other.ClientLCID &&
                   this.OffsetLength.Equals(other.OffsetLength);
        }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public void Pack(MemoryStream stream)
        {
            LittleEndianUtilities.WriteUInt(stream, this.Length);
            LittleEndianUtilities.WriteUInt(stream, this.TDSVersion);
            LittleEndianUtilities.WriteUInt(stream, this.PacketSize);
            LittleEndianUtilities.WriteUInt(stream, this.ClientProgVer);
            LittleEndianUtilities.WriteUInt(stream, this.ClientPID);
            LittleEndianUtilities.WriteUInt(stream, this.ConnectionID);
            this.OptionFlags1.Pack(stream);
            this.OptionFlags2.Pack(stream);
            this.TypeFlags.Pack(stream);
            this.OptionFlags3.Pack(stream);
            LittleEndianUtilities.WriteUInt(stream, 480); // Client time zone
            LittleEndianUtilities.WriteUInt(stream, 1033); // Client LCID
            this.OffsetLength.Pack(stream);
            stream.Write(this.Data, 0, this.Data.Length);

            // ToDo: Extensions not supported
        }

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
        public bool Unpack(MemoryStream stream)
        {
            LittleEndianUtilities.ReadUInt(stream);
            this.TDSVersion = LittleEndianUtilities.ReadUInt(stream);
            this.PacketSize = LittleEndianUtilities.ReadUInt(stream);
            this.ClientProgVer = LittleEndianUtilities.ReadUInt(stream);
            this.ClientPID = LittleEndianUtilities.ReadUInt(stream);
            this.ConnectionID = LittleEndianUtilities.ReadUInt(stream);
            this.OptionFlags1.Unpack(stream);
            this.OptionFlags2.Unpack(stream);
            this.TypeFlags.Unpack(stream);
            this.OptionFlags3.Unpack(stream);
            this.ClientTimeZone = Convert.ToInt32(LittleEndianUtilities.ReadUInt(stream));
            this.ClientLCID = LittleEndianUtilities.ReadUInt(stream);
            this.OffsetLength.Unpack(stream);

            this.Data = new byte[(int)this.OffsetLength.TotalLength()];
            stream.Read(this.Data, 0, (int)this.OffsetLength.TotalLength());

            // Extensions not supported
            return true;
        }

        /// <summary>
        /// TDS Login7 Data portion length.
        /// </summary>
        /// <returns>Returns TDS Login7 Data portion length.</returns>
        ushort ITDSPacketData.Length()
        {
            return Convert.ToUInt16(this.Length);
        }
    }
}
