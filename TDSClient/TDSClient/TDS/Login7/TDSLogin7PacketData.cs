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
        /// The total length of the LOGIN7 structure.
        /// </summary>
        public uint Length
        {
            get
            {
                return Convert.ToUInt32(7 * sizeof(uint) + sizeof(int) + 4 * sizeof(byte) + Data.Length + 24 * sizeof(ushort) + sizeof(uint) + 6 * sizeof(byte));
            }
        }

        /// <summary>
        /// The highest TDS version being used by the client.
        /// </summary>
        public uint TDSVersion { get; private set; }

        /// <summary>
        /// The packet size being requested by the client.
        /// </summary>
        public uint PacketSize { get; private set; }

        /// <summary>
        /// The version of the interface library (for example, ODBC or OLEDB) being used by the client.
        /// </summary>
        public uint ClientProgVer { get; private set; }

        /// <summary>
        /// The process ID of the client application.
        /// </summary>
        public uint ClientPID { get; private set; }

        /// <summary>
        /// The connection ID of the primary Server. Used when connecting to an "Always Up" backup
        /// server.
        /// </summary>
        public uint ConnectionID { get; private set; }

        public TDSLogin7OptionFlags1 OptionFlags1 { get; private set; }

        public TDSLogin7OptionFlags2 OptionFlags2 { get; private set; }

        public TDSLogin7TypeFlags TypeFlags { get; private set; }

        public TDSLogin7OptionFlags3 OptionFlags3 { get; private set; }

        /// <summary>
        /// This field is not used and can be set to zero.
        /// </summary>
        public int ClientTimeZone { get; private set; }

        /// <summary>
        /// The language code identifier (LCID) value for the client collation. 
        /// If ClientLCID is specified, the specified collation is set as the 
        /// session collation.
        /// </summary>
        public uint ClientLCID { get; private set; }

        /// <summary>
        /// The variable portion of this message. A stream of bytes in the order shown, indicates the offset
        /// (from the start of the message) and length of various parameters.
        /// </summary>
        public TDSLogin7OffsetLength OffsetLength { get; private set; }

        /// <summary>
        /// The actual variable-length data portion referred to by OffsetLength.
        /// </summary>
        private byte[] Data;

        public TDSLogin7PacketData()
        {
            OptionFlags1 = new TDSLogin7OptionFlags1();
            OptionFlags2 = new TDSLogin7OptionFlags2();
            OptionFlags3 = new TDSLogin7OptionFlags3();
            TypeFlags = new TDSLogin7TypeFlags();
            OffsetLength = new TDSLogin7OffsetLength();
        }

        // FeatureExt - unsupported
        public void AddOption(string optionName, ushort length, object data)
        {
            if (optionName == null || data == null)
            {
                throw new ArgumentNullException();
            }

            OffsetLength.AddOptionPositionInfo(optionName, length);
            var prevLength = Data != null ? Data.Length : 0;

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

            Array.Resize(ref Data, prevLength + optionData.Length);

            if (optionName == "Password")
            {
                for (int i = 0; i < optionData.Length; i++)
                {
                    var piece0 = (byte)(optionData[i] >> 4);
                    var piece1 = (byte)(optionData[i] & 0x0f);
                    optionData[i] = (byte)((piece0 | (piece1 << 4)) ^ 0xA5);
                }
            }

            Array.Copy(optionData, 0, Data, prevLength, optionData.Length);
        }

        public void Pack(MemoryStream stream)
        {
            LittleEndianUtilities.WriteUInt(stream, Length);
            LittleEndianUtilities.WriteUInt(stream, 1946157060); // 7.4 TDS Version
            LittleEndianUtilities.WriteUInt(stream, 4096); // PacketSize
            LittleEndianUtilities.WriteUInt(stream, 117440512); // ClientProgramVersion
            LittleEndianUtilities.WriteUInt(stream, (uint)Process.GetCurrentProcess().Id); // Client Process ID
            LittleEndianUtilities.WriteUInt(stream, 0); // Connection ID
            OptionFlags1.Pack(stream);
            OptionFlags2.Pack(stream);
            TypeFlags.Pack(stream);
            OptionFlags3.Pack(stream);
            LittleEndianUtilities.WriteUInt(stream, 480); // Client time zone
            LittleEndianUtilities.WriteUInt(stream, 1033); // Client LCID
            OffsetLength.Pack(stream);
            stream.Write(Data, 0, Data.Length);
            // Extensions extensions not supported
        }

        public bool Unpack(MemoryStream stream)
        {
            LittleEndianUtilities.ReadUInt(stream);
            TDSVersion = LittleEndianUtilities.ReadUInt(stream);
            PacketSize = LittleEndianUtilities.ReadUInt(stream);
            ClientProgVer = LittleEndianUtilities.ReadUInt(stream);
            ClientPID = LittleEndianUtilities.ReadUInt(stream);
            ConnectionID = LittleEndianUtilities.ReadUInt(stream);
            OptionFlags1.Unpack(stream);
            OptionFlags2.Unpack(stream);
            TypeFlags.Unpack(stream);
            OptionFlags3.Unpack(stream);
            ClientTimeZone = Convert.ToInt32(LittleEndianUtilities.ReadUInt(stream));
            ClientLCID = LittleEndianUtilities.ReadUInt(stream);
            OffsetLength.Unpack(stream);
            stream.Read(Data, 0, (int)OffsetLength.TotalLength());
            // Extensions not supported
            return true;
        }

        ushort ITDSPacketData.Length()
        {
            return Convert.ToUInt16(Length);
        }
    }
}
