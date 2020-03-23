//  ---------------------------------------------------------------------------
//  <copyright file="TDSLogin7PacketData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Login7
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Utilities;

    public class TDSLogin7PacketData : ITDSPacketData
    {
        /// <summary>
        /// The actual variable-length data portion referred to by OffsetLength.
        /// </summary>
        private byte[] data;

        public TDSLogin7PacketData()
        {
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
                return Convert.ToUInt32((7 * sizeof(uint)) + sizeof(int) + (4 * sizeof(byte)) + this.data.Length + (24 * sizeof(ushort)) + sizeof(uint) + (6 * sizeof(byte)));
            }
        }

        /// <summary>
        /// Gets the highest TDS version being used by the client.
        /// </summary>
        public uint TDSVersion { get; private set; }

        /// <summary>
        /// Gets the packet size being requested by the client.
        /// </summary>
        public uint PacketSize { get; private set; }

        /// <summary>
        /// Gets the version of the interface library (for example, ODBC or OLEDB) being used by the client.
        /// </summary>
        public uint ClientProgVer { get; private set; }

        /// <summary>
        /// Gets the process ID of the client application.
        /// </summary>
        public uint ClientPID { get; private set; }

        /// <summary>
        /// Gets the connection ID of the primary Server. Used when connecting to an "Always Up" backup
        /// server.
        /// </summary>
        public uint ConnectionID { get; private set; }

        /// <summary>
        /// Gets Option Flags 1.
        /// </summary>
        public TDSLogin7OptionFlags1 OptionFlags1 { get; private set; }

        /// <summary>
        /// Gets Option Flags 2.
        /// </summary>
        public TDSLogin7OptionFlags2 OptionFlags2 { get; private set; }

        /// <summary>
        /// Gets Type Flags.
        /// </summary>
        public TDSLogin7TypeFlags TypeFlags { get; private set; }

        /// <summary>
        /// Gets Option Flags 3.
        /// </summary>
        public TDSLogin7OptionFlags3 OptionFlags3 { get; private set; }

        /// <summary>
        /// Gets ClientTimeZone.
        /// This field is not used and can be set to zero.
        /// </summary>
        public int ClientTimeZone { get; private set; }

        /// <summary>
        /// Gets Client LCID.
        /// The language code identifier (LCID) value for the client collation. 
        /// If ClientLCID is specified, the specified collation is set as the 
        /// session collation.
        /// </summary>
        public uint ClientLCID { get; private set; }

        /// <summary>
        /// Gets the variable portion of this message. A stream of bytes in the order shown, indicates the offset
        /// (from the start of the message) and length of various parameters.
        /// </summary>
        public TDSLogin7OffsetLength OffsetLength { get; private set; }

        // FeatureExt - unsupported
        public void AddOption(string optionName, ushort length, object data)
        {
            if (optionName == null || data == null)
            {
                throw new ArgumentNullException();
            }

            this.OffsetLength.AddOptionPositionInfo(optionName, length);
            var prevLength = this.data != null ? this.data.Length : 0;

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

            Array.Resize(ref this.data, prevLength + optionData.Length);

            if (optionName == "Password")
            {
                for (int i = 0; i < optionData.Length; i++)
                {
                    var piece0 = (byte)(optionData[i] >> 4);
                    var piece1 = (byte)(optionData[i] & 0x0f);
                    optionData[i] = (byte)((piece0 | (piece1 << 4)) ^ 0xA5);
                }
            }

            Array.Copy(optionData, 0, this.data, prevLength, optionData.Length);
        }

        public void Pack(MemoryStream stream)
        {
            LittleEndianUtilities.WriteUInt(stream, this.Length);
            LittleEndianUtilities.WriteUInt(stream, 1946157060); // 7.4 TDS Version
            LittleEndianUtilities.WriteUInt(stream, 4096); // PacketSize
            LittleEndianUtilities.WriteUInt(stream, 117440512); // ClientProgramVersion
            LittleEndianUtilities.WriteUInt(stream, (uint)Process.GetCurrentProcess().Id); // Client Process ID
            LittleEndianUtilities.WriteUInt(stream, 0); // Connection ID
            this.OptionFlags1.Pack(stream);
            this.OptionFlags2.Pack(stream);
            this.TypeFlags.Pack(stream);
            this.OptionFlags3.Pack(stream);
            LittleEndianUtilities.WriteUInt(stream, 480); // Client time zone
            LittleEndianUtilities.WriteUInt(stream, 1033); // Client LCID
            this.OffsetLength.Pack(stream);
            stream.Write(this.data, 0, this.data.Length);

            // ToDo: Extensions not supported
        }

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
            stream.Read(this.data, 0, (int)this.OffsetLength.TotalLength());

            // Extensions not supported
            return true;
        }

        ushort ITDSPacketData.Length()
        {
            return Convert.ToUInt16(this.Length);
        }
    }
}
