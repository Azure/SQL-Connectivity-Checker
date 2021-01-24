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
    using System.Runtime.CompilerServices;
    using System.Text;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Login7.Options;
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
        /// <param name="tdsVersion">TDS Version</param>
        /// <param name="packetSize">Packet size</param>
        /// <param name="clientProgVer">Client program version</param>
        /// <param name="connectionID">Connection ID</param>
        /// <param name="clientLCID">Client LCID</param>
        /// <param name="clientID">Client ID</param>
        public TDSLogin7PacketData(uint tdsVersion = 1946157060, uint packetSize = 4096, uint clientProgVer = 117440512, uint connectionID = 0, uint clientLCID = 1033, byte[] clientID = null)
        {
            this.TDSVersion = tdsVersion;
            this.PacketSize = packetSize;
            this.ClientProgVer = clientProgVer;
            this.ClientPID = (uint)Process.GetCurrentProcess().Id;
            this.ConnectionID = connectionID;
            this.ClientLCID = clientLCID;

            this.OptionFlags1 = new TDSLogin7OptionFlags1();
            this.OptionFlags2 = new TDSLogin7OptionFlags2();
            this.OptionFlags3 = new TDSLogin7OptionFlags3();
            this.TypeFlags = new TDSLogin7TypeFlags();
            this.Options = new List<TDSLogin7Option>();

            if (clientID == null)
            {
                this.ClientID = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05 };
            }
            else if (clientID.Length != 6)
            {
                throw new Exception("Invalid ClientID length!");
            }
            else
            {
                this.ClientID = clientID;
            }
        }

        /// <summary>
        /// Gets the total length of the LOGIN7 structure.
        /// </summary>
        public uint Length
        {
            get
            {
                return Convert.ToUInt32((8 * sizeof(uint)) + (4 * sizeof(byte)) + this.Options.Sum(opt => opt.TrueLength) + (24 * sizeof(ushort)) + sizeof(uint) + (6 * sizeof(byte)));
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
        public uint ClientTimeZone { get; set; }

        /// <summary>
        /// Gets or sets Client LCID.
        /// The language code identifier (LCID) value for the client collation. 
        /// If ClientLCID is specified, the specified collation is set as the 
        /// session collation.
        /// </summary>
        public uint ClientLCID { get; set; }

        /// <summary>
        /// Gets or sets Client ID
        /// </summary>
        public byte[] ClientID { get; set; }

        /// <summary>
        /// Gets or sets the variable portion of this message. A stream of bytes in the order shown, indicates the offset
        /// (from the start of the message) and length of various parameters.
        /// </summary>
        public List<TDSLogin7Option> Options { get; set; }

        /// <summary>
        /// Add TDS Login7 Option.
        /// </summary>
        /// <param name="optionName">Option Name</param>
        /// <param name="data">Option Data</param>
        public void AddOption(string optionName, string data)
        {
            if (optionName == null || data == null)
            {
                throw new ArgumentNullException();
            }

            if (this.Options.Where(opt => opt.Name == optionName).Any())
            {
                throw new Exception("Login7 option already set!");
            }

            if (optionName != "Password" && optionName != "ChangePassword")
            {
                LoggingUtilities.WriteLog($"  Adding Login7 option {optionName} [{data}].");
            }
            else
            {
                LoggingUtilities.WriteLog($"  Adding Login7 option {optionName}.");
            }

            var option = TDSLogin7OptionFactory.CreateOption(optionName, data);

            this.Options.Add(option);
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
                   this.ClientID.SequenceEqual(other.ClientID) &&
                   this.Options.All(other.Options.Contains);
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
            LittleEndianUtilities.WriteUInt(stream, this.ClientTimeZone);
            LittleEndianUtilities.WriteUInt(stream, this.ClientLCID);
            TDSLogin7OptionFactory.WriteOptionsToStream(stream, this.Options, this.ClientID);
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
            this.ClientTimeZone = Convert.ToUInt32(LittleEndianUtilities.ReadUInt(stream));
            this.ClientLCID = LittleEndianUtilities.ReadUInt(stream);

            var result = TDSLogin7OptionFactory.ReadOptionsFromStream(stream);
            this.ClientID = result.Item2;
            this.Options = result.Item1;

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