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

    public class TDSLogin7OffsetLength : IPackageable
    {
        /// <summary>
        ///  The client machine name (position).
        /// </summary>
        public ushort HostNamePosition { get; set; }

        /// <summary>
        ///  The client machine name (length).
        /// </summary>
        public ushort HostNameLength { get; set; }

        /// <summary>
        ///  The client user ID (position).
        /// </summary>
        public ushort UserNamePosition { get; set; }

        /// <summary>
        ///  The client user ID (length).
        /// </summary>
        public ushort UserNameLength { get; set; }

        /// <summary>
        /// The password supplied by the client (position).
        /// </summary>
        public ushort PasswordPosition { get; set; }

        /// <summary>
        /// The password supplied by the client (length).
        /// </summary>
        public ushort PasswordLength { get; set; }

        /// <summary>
        ///  The client application name (position).
        /// </summary>
        public ushort AppNamePosition { get; set; }

        /// <summary>
        ///  The client application name (length).
        /// </summary>
        public ushort AppNameLength { get; set; }

        /// <summary>
        ///  The server name (position).
        /// </summary>
        public ushort ServerNamePosition { get; set; }

        /// <summary>
        ///  The server name (length).
        /// </summary>
        public ushort ServerNameLength { get; set; }

        /// <summary>
        ///  This points to an extension block. Introduced in TDS 7.4 when fExtension is 1 (position). (Unsupported)
        /// </summary>
        public ushort ExtensionPosition { get; set; }

        /// <summary>
        ///  This points to an extension block. Introduced in TDS 7.4 when fExtension is 1 (length). (Unsupported)
        /// </summary>
        public ushort ExtensionLength { get; set; }

        /// <summary>
        /// The interface library name (ODBC or OLEDB) (position).
        /// </summary>
        public ushort CltIntNamePosition { get; set; }

        /// <summary>
        ///  The interface library name (ODBC or OLEDB) (length).
        /// </summary>
        public ushort CltIntNameLength { get; set; }

        /// <summary>
        /// The initial language (overrides the user ID's default
        /// language) (position).
        /// </summary>
        public ushort LanguagePosition { get; set; }

        /// <summary>
        /// The initial language (overrides the user ID's default
        /// language) (length).
        /// </summary>
        public ushort LanguageLength { get; set; }

        /// <summary>
        /// The initial database (overrides the user ID's default database) (position).
        /// </summary>
        public ushort DatabasePosition { get; set; }

        /// <summary>
        /// The initial database (overrides the user ID's default database) (length).
        /// </summary>
        public ushort DatabaseLength { get; set; }

        /// <summary>
        /// The unique client ID (created by using the NIC address). ClientID is the MAC
        /// address of the physical network layer.It is used to identify the client that is connecting to
        /// the server. This value is mainly informational, and no processing steps on the server side
        /// use it.
        /// </summary>
        public byte[] ClientID { get; set; } // byte[6]

        /// <summary>
        ///  SSPI data.
        /// If SSPILength < USHORT_MAX, then this length MUST be used for SSPI and SSPILengthLong
        /// MUST be ignored.
        /// If SSPILength == USHORT_MAX, then SSPILengthLong MUST be checked.
        /// If SSPILengthLong > 0, then that value MUST be used. If SSPILengthLong == 0, then SSPILength
        /// (USHORT_MAX) MUST be used. (position)
        /// (Unsupported)
        /// </summary>
        public ushort SSPIPosition { get; set; }

        /// <summary>
        ///  SSPI data.
        /// If SSPILength < USHORT_MAX, then this length MUST be used for SSPI and SSPILengthLong
        /// MUST be ignored.
        /// If SSPILength == USHORT_MAX, then SSPILengthLong MUST be checked.
        /// If SSPILengthLong > 0, then that value MUST be used. If SSPILengthLong == 0, then SSPILength
        /// (USHORT_MAX) MUST be used. (length) 
        /// (Unsupported)
        /// </summary>
        public ushort SSPILength { get; set; }

        /// <summary>
        /// The file name for a database that is to be attached during the connection process (position).
        /// </summary>
        public ushort AtchDBFilePosition { get; set; }

        /// <summary>
        /// The file name for a database that is to be attached during the connection process (length).
        /// </summary>
        public ushort AtchDBFileLength { get; set; }

        /// <summary>
        ///  New password for the specified login (position).
        /// </summary>
        public ushort ChangePasswordPosition { get; set; }

        /// <summary>
        ///  New password for the specified login (length).
        /// </summary>
        public ushort ChangePasswordLength { get; set; }

        /// <summary>
        /// Used for large SSPI data a when SSPILength==USHORT_MAX.
        /// </summary>
        public uint SSPILengthLong { get; set; }

        private ushort LastPos = 94; // Fixed portion length

        public TDSLogin7OffsetLength()
        {
            ClientID = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6 };
        }

        public void AddOptionPositionInfo(string optionName, ushort length)
        {
            switch (optionName)
            {
                case "HostName":
                    {
                        HostNamePosition = LastPos;
                        HostNameLength = length;
                        LastPos += (ushort)(length * 2);
                        break;
                    }

                case "UserName":
                    {
                        UserNamePosition = LastPos;
                        UserNameLength = length;
                        LastPos += (ushort)(length * 2);
                        break;
                    }

                case "Password":
                    {
                        PasswordPosition = LastPos;
                        PasswordLength = length;
                        LastPos += (ushort)(length * 2);
                        break;
                    }

                case "AppName":
                    {
                        AppNamePosition = LastPos;
                        AppNameLength = length;
                        LastPos += (ushort)(length * 2);
                        break;
                    }

                case "ServerName":
                    {
                        ServerNamePosition = LastPos;
                        ServerNameLength = length;
                        LastPos += (ushort)(length * 2);
                        break;
                    }

                case "Extension":
                    {
                        throw new NotSupportedException();
                    }

                case "IntName":
                    {
                        CltIntNamePosition = LastPos;
                        CltIntNameLength = length;
                        LastPos += (ushort)(length * 2);
                        break;
                    }

                case "Language":
                    {
                        LanguagePosition = LastPos;
                        LanguageLength = length;
                        LastPos += (ushort)(length * 2);
                        break;
                    }

                case "Database":
                    {
                        DatabasePosition = LastPos;
                        DatabaseLength = length;
                        LastPos += (ushort)(length * 2);
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

        public uint TotalLength()
        {
            return Convert.ToUInt32((ChangePasswordLength + AtchDBFileLength + DatabaseLength + LanguageLength + CltIntNameLength + ServerNameLength + AppNameLength + PasswordLength
                + UserNameLength + HostNameLength) * 2 + SSPILength) + ExtensionLength + SSPILengthLong;
        }

        public void Pack(MemoryStream stream)
        {
            LittleEndianUtilities.WriteUShort(stream, HostNamePosition);
            LittleEndianUtilities.WriteUShort(stream, HostNameLength);
            LittleEndianUtilities.WriteUShort(stream, UserNamePosition);
            LittleEndianUtilities.WriteUShort(stream, UserNameLength);
            LittleEndianUtilities.WriteUShort(stream, PasswordPosition);
            LittleEndianUtilities.WriteUShort(stream, PasswordLength);
            LittleEndianUtilities.WriteUShort(stream, AppNamePosition);
            LittleEndianUtilities.WriteUShort(stream, AppNameLength);
            LittleEndianUtilities.WriteUShort(stream, ServerNamePosition);
            LittleEndianUtilities.WriteUShort(stream, ServerNameLength);
            LittleEndianUtilities.WriteUShort(stream, 0); // Extension unsupported
            LittleEndianUtilities.WriteUShort(stream, 0); // Extension unsupported
            LittleEndianUtilities.WriteUShort(stream, CltIntNamePosition);
            LittleEndianUtilities.WriteUShort(stream, CltIntNameLength);
            LittleEndianUtilities.WriteUShort(stream, LanguagePosition);
            LittleEndianUtilities.WriteUShort(stream, LanguageLength);
            LittleEndianUtilities.WriteUShort(stream, DatabasePosition);
            LittleEndianUtilities.WriteUShort(stream, DatabaseLength);
            stream.Write(ClientID, 0, 6);
            LittleEndianUtilities.WriteUShort(stream, SSPIPosition);
            LittleEndianUtilities.WriteUShort(stream, SSPILength);
            LittleEndianUtilities.WriteUShort(stream, AtchDBFilePosition);
            LittleEndianUtilities.WriteUShort(stream, AtchDBFileLength);
            LittleEndianUtilities.WriteUShort(stream, ChangePasswordPosition);
            LittleEndianUtilities.WriteUShort(stream, ChangePasswordLength);
            LittleEndianUtilities.WriteUInt(stream, 0); // Long SSPI not supported
        }

        public bool Unpack(MemoryStream stream)
        {
            HostNamePosition = LittleEndianUtilities.ReadUShort(stream);
            HostNameLength = LittleEndianUtilities.ReadUShort(stream);
            UserNamePosition = LittleEndianUtilities.ReadUShort(stream);
            UserNameLength = LittleEndianUtilities.ReadUShort(stream);
            PasswordPosition = LittleEndianUtilities.ReadUShort(stream);
            PasswordLength = LittleEndianUtilities.ReadUShort(stream);
            AppNamePosition = LittleEndianUtilities.ReadUShort(stream);
            AppNameLength = LittleEndianUtilities.ReadUShort(stream);
            ServerNamePosition = LittleEndianUtilities.ReadUShort(stream);
            ServerNameLength = LittleEndianUtilities.ReadUShort(stream);
            ExtensionPosition = LittleEndianUtilities.ReadUShort(stream);
            ExtensionLength = LittleEndianUtilities.ReadUShort(stream);
            CltIntNamePosition = LittleEndianUtilities.ReadUShort(stream);
            CltIntNameLength = LittleEndianUtilities.ReadUShort(stream);
            LanguagePosition = LittleEndianUtilities.ReadUShort(stream);
            LanguageLength = LittleEndianUtilities.ReadUShort(stream);
            DatabasePosition = LittleEndianUtilities.ReadUShort(stream);
            DatabaseLength = LittleEndianUtilities.ReadUShort(stream);
            stream.Read(ClientID, 0, 6);
            SSPIPosition = LittleEndianUtilities.ReadUShort(stream);
            SSPILength = LittleEndianUtilities.ReadUShort(stream);
            AtchDBFilePosition = LittleEndianUtilities.ReadUShort(stream);
            AtchDBFileLength = LittleEndianUtilities.ReadUShort(stream);
            ChangePasswordPosition = LittleEndianUtilities.ReadUShort(stream);
            ChangePasswordLength = LittleEndianUtilities.ReadUShort(stream);
            SSPILengthLong = LittleEndianUtilities.ReadUInt(stream);
            return true;
        }
    }
}
