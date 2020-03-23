//  ---------------------------------------------------------------------------
//  <copyright file="TDSLogin7OffsetLength.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Login7
{
    using System;
    using System.IO;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Utilities;

    /// <summary>
    /// Class describing variable length portion of the TDS Login7 packet
    /// </summary>
    public class TDSLogin7OffsetLength : IPackageable
    {
        /// <summary>
        /// Last Position written to within the data portion of TDS Login7 Packet.
        /// Initialized to fixed portion length of 94 bytes.
        /// </summary>
        private ushort lastPos = 94;

        /// <summary>
        /// Initializes a new instance of the <see cref="TDSLogin7OffsetLength" /> class.
        /// </summary>
        public TDSLogin7OffsetLength()
        {
            this.ClientID = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6 };
        }

        /// <summary>
        ///  Gets or sets the client machine name (position).
        /// </summary>
        public ushort HostNamePosition { get; set; }

        /// <summary>
        ///  Gets or sets the client machine name (length).
        /// </summary>
        public ushort HostNameLength { get; set; }

        /// <summary>
        ///  Gets or sets the client user ID (position).
        /// </summary>
        public ushort UserNamePosition { get; set; }

        /// <summary>
        ///  Gets or sets the client user ID (length).
        /// </summary>
        public ushort UserNameLength { get; set; }

        /// <summary>
        /// Gets or sets the password supplied by the client (position).
        /// </summary>
        public ushort PasswordPosition { get; set; }

        /// <summary>
        /// Gets or sets the password supplied by the client (length).
        /// </summary>
        public ushort PasswordLength { get; set; }

        /// <summary>
        ///  Gets or sets the client application name (position).
        /// </summary>
        public ushort AppNamePosition { get; set; }

        /// <summary>
        ///  Gets or sets the client application name (length).
        /// </summary>
        public ushort AppNameLength { get; set; }

        /// <summary>
        ///  Gets or sets the server name (position).
        /// </summary>
        public ushort ServerNamePosition { get; set; }

        /// <summary>
        ///  Gets or sets the server name (length).
        /// </summary>
        public ushort ServerNameLength { get; set; }

        /// <summary>
        /// Gets or sets ExtensionPosition. 
        /// This points to an extension block. Introduced in TDS 7.4 when fExtension is 1 (position). (Unsupported)
        /// </summary>
        public ushort ExtensionPosition { get; set; }

        /// <summary>
        /// Gets or sets ExtensionLength.
        ///  This points to an extension block. Introduced in TDS 7.4 when fExtension is 1 (length). (Unsupported)
        /// </summary>
        public ushort ExtensionLength { get; set; }

        /// <summary>
        /// Gets or sets the interface library name (ODBC or OLEDB) (position).
        /// </summary>
        public ushort CltIntNamePosition { get; set; }

        /// <summary>
        ///  Gets or sets the interface library name (ODBC or OLEDB) (length).
        /// </summary>
        public ushort CltIntNameLength { get; set; }

        /// <summary>
        /// Gets or sets the initial language (overrides the user ID's default
        /// language) (position).
        /// </summary>
        public ushort LanguagePosition { get; set; }

        /// <summary>
        /// Gets or sets the initial language (overrides the user ID's default
        /// language) (length).
        /// </summary>
        public ushort LanguageLength { get; set; }

        /// <summary>
        /// Gets or sets the initial database (overrides the user ID's default database) (position).
        /// </summary>
        public ushort DatabasePosition { get; set; }

        /// <summary>
        /// Gets or sets the initial database (overrides the user ID's default database) (length).
        /// </summary>
        public ushort DatabaseLength { get; set; }

        /// <summary>
        /// Gets or sets the unique client ID (created by using the NIC address). ClientID is the MAC
        /// address of the physical network layer.It is used to identify the client that is connecting to
        /// the server. This value is mainly informational, and no processing steps on the server side
        /// use it.
        /// </summary>
        public byte[] ClientID { get; set; } // byte[6]

        /// <summary>
        /// Gets or sets SSPI data.
        /// If SSPILength is less than USHORT_MAX, then this length MUST be used for SSPI and SSPILengthLong
        /// MUST be ignored.
        /// If SSPILength == USHORT_MAX, then SSPILengthLong MUST be checked.
        /// If SSPILengthLong is greater than 0, then that value MUST be used. If SSPILengthLong == 0, then SSPILength
        /// (USHORT_MAX) MUST be used. (position)
        /// (Unsupported)
        /// </summary>
        public ushort SSPIPosition { get; set; }

        /// <summary>
        /// Gets or sets SSPI data.
        /// If SSPILength is less than USHORT_MAX, then this length MUST be used for SSPI and SSPILengthLong
        /// MUST be ignored.
        /// If SSPILength == USHORT_MAX, then SSPILengthLong MUST be checked.
        /// If SSPILengthLong is greater than 0, then that value MUST be used. If SSPILengthLong == 0, then SSPILength
        /// (USHORT_MAX) MUST be used. (length) 
        /// (Unsupported)
        /// </summary>
        public ushort SSPILength { get; set; }

        /// <summary>
        /// Gets or sets the file name for a database that is to be attached during the connection process (position).
        /// </summary>
        public ushort AtchDBFilePosition { get; set; }

        /// <summary>
        /// Gets or sets the file name for a database that is to be attached during the connection process (length).
        /// </summary>
        public ushort AtchDBFileLength { get; set; }

        /// <summary>
        /// Gets or sets the new password for the specified login (position).
        /// </summary>
        public ushort ChangePasswordPosition { get; set; }

        /// <summary>
        ///  Gets or sets the new password for the specified login (length).
        /// </summary>
        public ushort ChangePasswordLength { get; set; }

        /// <summary>
        /// Gets or sets the field used to specify length for large SSPI data a when SSPILength==USHORT_MAX.
        /// </summary>
        public uint SSPILengthLong { get; set; }

        /// <summary>
        /// Adds TDS Login7 Option Info.
        /// </summary>
        /// <param name="optionName">Option Name</param>
        /// <param name="length">Option Length (in bytes)</param>
        public void AddOptionPositionInfo(string optionName, ushort length)
        {
            switch (optionName)
            {
                case "HostName":
                    {
                        this.HostNamePosition = this.lastPos;
                        this.HostNameLength = length;
                        this.lastPos += (ushort)(length * 2);
                        break;
                    }

                case "UserName":
                    {
                        this.UserNamePosition = this.lastPos;
                        this.UserNameLength = length;
                        this.lastPos += (ushort)(length * 2);
                        break;
                    }

                case "Password":
                    {
                        this.PasswordPosition = this.lastPos;
                        this.PasswordLength = length;
                        this.lastPos += (ushort)(length * 2);
                        break;
                    }

                case "AppName":
                    {
                        this.AppNamePosition = this.lastPos;
                        this.AppNameLength = length;
                        this.lastPos += (ushort)(length * 2);
                        break;
                    }

                case "ServerName":
                    {
                        this.ServerNamePosition = this.lastPos;
                        this.ServerNameLength = length;
                        this.lastPos += (ushort)(length * 2);
                        break;
                    }

                case "Extension":
                    {
                        throw new NotSupportedException();
                    }

                case "IntName":
                    {
                        this.CltIntNamePosition = this.lastPos;
                        this.CltIntNameLength = length;
                        this.lastPos += (ushort)(length * 2);
                        break;
                    }

                case "Language":
                    {
                        this.LanguagePosition = this.lastPos;
                        this.LanguageLength = length;
                        this.lastPos += (ushort)(length * 2);
                        break;
                    }

                case "Database":
                    {
                        this.DatabasePosition = this.lastPos;
                        this.DatabaseLength = length;
                        this.lastPos += (ushort)(length * 2);
                        break;
                    }

                case "SSPI":
                    {
                        throw new NotSupportedException();
                    }

                case "AtchDBFile":
                    {
                        throw new NotSupportedException();
                    }

                case "ChangePassword":
                    {
                        throw new NotSupportedException();
                    }

                default:
                    {
                        throw new InvalidOperationException();
                    }
            }
        }

        /// <summary>
        /// Total data portion length.
        /// </summary>
        /// <returns>Returns total length.</returns>
        public uint TotalLength()
        {
            return Convert.ToUInt32(
                ((this.ChangePasswordLength +
                this.AtchDBFileLength +
                this.DatabaseLength +
                this.LanguageLength +
                this.CltIntNameLength +
                this.ServerNameLength +
                this.AppNameLength +
                this.PasswordLength +
                this.UserNameLength +
                this.HostNameLength) * 2) +
                this.SSPILength) +
                this.ExtensionLength +
                this.SSPILengthLong;
        }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public void Pack(MemoryStream stream)
        {
            LittleEndianUtilities.WriteUShort(stream, this.HostNamePosition);
            LittleEndianUtilities.WriteUShort(stream, this.HostNameLength);
            LittleEndianUtilities.WriteUShort(stream, this.UserNameLength);
            LittleEndianUtilities.WriteUShort(stream, this.PasswordPosition);
            LittleEndianUtilities.WriteUShort(stream, this.PasswordLength);
            LittleEndianUtilities.WriteUShort(stream, this.AppNamePosition);
            LittleEndianUtilities.WriteUShort(stream, this.AppNameLength);
            LittleEndianUtilities.WriteUShort(stream, this.ServerNamePosition);
            LittleEndianUtilities.WriteUShort(stream, this.ServerNameLength);
            LittleEndianUtilities.WriteUShort(stream, 0); // Extension unsupported
            LittleEndianUtilities.WriteUShort(stream, 0); // Extension unsupported
            LittleEndianUtilities.WriteUShort(stream, this.CltIntNamePosition);
            LittleEndianUtilities.WriteUShort(stream, this.CltIntNameLength);
            LittleEndianUtilities.WriteUShort(stream, this.LanguagePosition);
            LittleEndianUtilities.WriteUShort(stream, this.LanguageLength);
            LittleEndianUtilities.WriteUShort(stream, this.DatabasePosition);
            LittleEndianUtilities.WriteUShort(stream, this.DatabaseLength);
            stream.Write(this.ClientID, 0, 6);
            LittleEndianUtilities.WriteUShort(stream, this.SSPIPosition);
            LittleEndianUtilities.WriteUShort(stream, this.SSPILength);
            LittleEndianUtilities.WriteUShort(stream, this.AtchDBFilePosition);
            LittleEndianUtilities.WriteUShort(stream, this.AtchDBFileLength);
            LittleEndianUtilities.WriteUShort(stream, this.ChangePasswordPosition);
            LittleEndianUtilities.WriteUShort(stream, this.ChangePasswordLength);
            LittleEndianUtilities.WriteUInt(stream, 0); // Long SSPI not supported
        }

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
        public bool Unpack(MemoryStream stream)
        {
            this.HostNamePosition = LittleEndianUtilities.ReadUShort(stream);
            this.HostNameLength = LittleEndianUtilities.ReadUShort(stream);
            this.UserNamePosition = LittleEndianUtilities.ReadUShort(stream);
            this.UserNameLength = LittleEndianUtilities.ReadUShort(stream);
            this.PasswordPosition = LittleEndianUtilities.ReadUShort(stream);
            this.PasswordLength = LittleEndianUtilities.ReadUShort(stream);
            this.AppNamePosition = LittleEndianUtilities.ReadUShort(stream);
            this.AppNameLength = LittleEndianUtilities.ReadUShort(stream);
            this.ServerNamePosition = LittleEndianUtilities.ReadUShort(stream);
            this.ServerNameLength = LittleEndianUtilities.ReadUShort(stream);
            this.ExtensionPosition = LittleEndianUtilities.ReadUShort(stream);
            this.ExtensionLength = LittleEndianUtilities.ReadUShort(stream);
            this.CltIntNamePosition = LittleEndianUtilities.ReadUShort(stream);
            this.CltIntNameLength = LittleEndianUtilities.ReadUShort(stream);
            this.LanguagePosition = LittleEndianUtilities.ReadUShort(stream);
            this.LanguageLength = LittleEndianUtilities.ReadUShort(stream);
            this.DatabasePosition = LittleEndianUtilities.ReadUShort(stream);
            this.DatabaseLength = LittleEndianUtilities.ReadUShort(stream);
            stream.Read(this.ClientID, 0, 6);
            this.SSPIPosition = LittleEndianUtilities.ReadUShort(stream);
            this.SSPILength = LittleEndianUtilities.ReadUShort(stream);
            this.AtchDBFilePosition = LittleEndianUtilities.ReadUShort(stream);
            this.AtchDBFileLength = LittleEndianUtilities.ReadUShort(stream);
            this.ChangePasswordPosition = LittleEndianUtilities.ReadUShort(stream);
            this.ChangePasswordLength = LittleEndianUtilities.ReadUShort(stream);
            this.SSPILengthLong = LittleEndianUtilities.ReadUInt(stream);
            
            return true;
        }
    }
}
