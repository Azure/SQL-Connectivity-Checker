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

    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Login7.Options;
    using TDSClient.TDS.PreLogin;
    using TDSClient.TDS.Utilities;
    using static TDSClient.AuthenticationProvider.AuthenticationProvider;

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
        public TDSLogin7PacketData(
            string hostName,
            string appName,
            string server,
            string database,
            uint tdsVersion = 1946157060,
            uint packetSize = 4096,
            uint clientProgVer = 117440512,
            uint connectionID = 0,
            uint clientLCID = 1033,
            byte[] clientID = null)
        {
            TDSVersion = tdsVersion;
            PacketSize = packetSize;
            ClientProgVer = clientProgVer;
            ClientPID = (uint)Process.GetCurrentProcess().Id;
            ConnectionID = connectionID;
            ClientLCID = clientLCID;

            OptionFlags1 = new TDSLogin7OptionFlags1();
            OptionFlags2 = new TDSLogin7OptionFlags2();
            OptionFlags3 = new TDSLogin7OptionFlags3();
            TypeFlags = new TDSLogin7TypeFlags();
            Options = new List<TDSLogin7Option>();

            AddLogin7CommonOptions();

            if (clientID == null)
            {
                ClientID = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05 };
            }
            else if (clientID.Length != 6)
            {
                throw new Exception("Invalid ClientID length!");
            }
            else
            {
                ClientID = clientID;
            }

            LoggingUtilities.WriteLog($" Adding option HostName with value [{Environment.MachineName}]");
            HostName = Environment.MachineName;

            LoggingUtilities.WriteLog($"  Adding option ApplicationName with value [{appName}]");
            ApplicationName = hostName;

            LoggingUtilities.WriteLog($"  Adding option ServerName with value [{server}]");
            ServerName = server;

            LoggingUtilities.WriteLog($"  Adding option Database with value [{database}]");
            Database = database;

            ClientTimeZone = 480;
        }

        /// <summary>
        /// Adds options for SQL Authentication to TDS Login message.
        /// </summary>
        /// <param name="tdsMessageBody"></param>

        public void AddLogin7SQLAuthenticationOptions(string userId, string password)
        {
            LoggingUtilities.WriteLog($"  Adding option UserID with value [{userId}]");
            UserID = userId;

            LoggingUtilities.WriteLog($"  Adding option Password");
            Password = password;

            OptionFlags3.Extension = TDSLogin7OptionFlags3Extension.DoesntExist;
        }

        /// <summary>
        /// Adds options for AAD Authentication to TDS Login message.
        /// </summary>
        /// <param name="tdsMessageBody"></param>
        public void AddLogin7AADAuthenticationOptions(TDSAuthenticationType authenticationType)
        {
            TDSFedAuthADALWorkflow adalWorkflow = authenticationType.Equals(TDSAuthenticationType.ADIntegrated) ?
                TDSFedAuthADALWorkflow.Integrated : TDSFedAuthADALWorkflow.UserPassword;

            OptionFlags3.Extension = TDSLogin7OptionFlags3Extension.Exists;

            TDSLogin7FedAuthOptionToken featureOption = CreateLogin7FederatedAuthenticationFeatureExt(TDSFedAuthLibraryType.ADAL, adalWorkflow);

            FeatureExt ??= new TDSLogin7FeatureOptionsToken();
            FeatureExt.Add(featureOption);
        }

        /// <summary>
        /// Creates Fedauth feature extension for the login7 packet.
        /// </summary>
        private TDSLogin7FedAuthOptionToken CreateLogin7FederatedAuthenticationFeatureExt(
            TDSFedAuthLibraryType libraryType,
            TDSFedAuthADALWorkflow workflow = TDSFedAuthADALWorkflow.EMPTY)
        {
            TDSLogin7FedAuthOptionToken featureOption =
                new TDSLogin7FedAuthOptionToken(
                    TdsPreLoginFedAuthRequiredOption.FedAuthRequired,
                    libraryType,
                    null,
                    null,
                    null,
                    false,
                    libraryType == TDSFedAuthLibraryType.ADAL,
                    workflow);

            return featureOption;
        }

        /// <summary>
        /// Helper method to add common login 7 options to Login7 message.
        /// </summary>
        /// <param name="tdsMessageBody"></param>
        private void AddLogin7CommonOptions()
        {
            OptionFlags1.Char = TDSLogin7OptionFlags1Char.CharsetASCII;
            OptionFlags1.Database = TDSLogin7OptionFlags1Database.InitDBFatal;
            OptionFlags1.DumpLoad = TDSLogin7OptionFlags1DumpLoad.DumploadOn;
            OptionFlags1.Float = TDSLogin7OptionFlags1Float.FloatIEEE754;
            OptionFlags1.SetLang = TDSLogin7OptionFlags1SetLang.SetLangOn;
            OptionFlags1.ByteOrder = TDSLogin7OptionFlags1ByteOrder.OrderX86;
            OptionFlags1.UseDB = TDSLogin7OptionFlags1UseDB.UseDBOff;

            OptionFlags2.Language = TDSLogin7OptionFlags2Language.InitLangFatal;
            OptionFlags2.ODBC = TDSLogin7OptionFlags2ODBC.OdbcOn;
            OptionFlags2.UserType = TDSLogin7OptionFlags2UserType.UserNormal;
            OptionFlags2.IntSecurity = TDSLogin7OptionFlags2IntSecurity.IntegratedSecurityOff;

            OptionFlags3.ChangePassword = TDSLogin7OptionFlags3ChangePassword.NoChangeRequest;
            OptionFlags3.UserInstanceProcess = TDSLogin7OptionFlags3UserInstanceProcess.DontRequestSeparateProcess;
            OptionFlags3.UnknownCollationHandling = TDSLogin7OptionFlags3UnknownCollationHandling.On;

            TypeFlags.OLEDB = TDSLogin7TypeFlagsOLEDB.On;
            TypeFlags.SQLType = TDSLogin7TypeFlagsSQLType.DFLT;
            TypeFlags.ReadOnlyIntent = TDSLogin7TypeFlagsReadOnlyIntent.On;
        }

        /// <summary>
        /// Length of the fixed portion of the packet
        /// </summary>
        private static readonly ushort FixedLength = 
                  sizeof(uint)  // Length
                + sizeof(uint)  // TDSVersion
                + sizeof(uint)  // PacketSize
                + sizeof(uint)  // ClientProgramVersion
                + sizeof(uint)  // ClientPID
                + sizeof(uint)  // ConnectionID
                + sizeof(byte)  // OptionalFlags1
                + sizeof(byte)  // OptionalFlags2
                + sizeof(byte)  // OptionalFlags3
                + sizeof(byte)  // TypeFlags
                + sizeof(uint)  // ClientTimeZone
                + sizeof(int)  // ClientLCID
                + sizeof(ushort) + sizeof(ushort)  // HostName
                + sizeof(ushort) + sizeof(ushort)  // UserID
                + sizeof(ushort) + sizeof(ushort)  // Password
                + sizeof(ushort) + sizeof(ushort)  // ApplicationName
                + sizeof(ushort) + sizeof(ushort)  // ServerName
                + sizeof(ushort) + sizeof(ushort)  // Unused/Extension
                + sizeof(ushort) + sizeof(ushort)  // LibraryName
                + sizeof(ushort) + sizeof(ushort)  // Language
                + sizeof(ushort) + sizeof(ushort)  // Database
                + 6 * sizeof(byte)  // ClientID
                + sizeof(ushort) + sizeof(ushort)  // SSPI
                + sizeof(ushort) + sizeof(ushort)  // AttachDatabaseFile
                + sizeof(ushort) + sizeof(ushort)  // ChangePassword
                + sizeof(int);  // LongSSPI;

        /// <summary>
        /// Gets the length of the Login7 structure
        /// </summary>
        public ushort Length()
        {
            uint notFixed = 
                  (uint)(string.IsNullOrEmpty(HostName) ? 0 : HostName.Length * 2)
                + (uint)(string.IsNullOrEmpty(UserID) ? 0 : UserID.Length * 2)
                + (uint)(string.IsNullOrEmpty(Password) ? 0 : Password.Length * 2)
                + (uint)(string.IsNullOrEmpty(ApplicationName) ? 0 : ApplicationName.Length * 2)
                + (uint)(string.IsNullOrEmpty(ServerName) ? 0 : ServerName.Length * 2)
                + (uint)(string.IsNullOrEmpty(LibraryName) ? 0 : LibraryName.Length * 2)
                + (uint)(string.IsNullOrEmpty(Language) ? 0 : Language.Length * 2)
                + (uint)(string.IsNullOrEmpty(Database) ? 0 : Database.Length * 2)
                + (uint)(string.IsNullOrEmpty(AttachDatabaseFile) ? 0 : AttachDatabaseFile.Length * 2) 
                + (uint)(string.IsNullOrEmpty(ChangePassword) ? 0 : ChangePassword.Length * 2) 
                + (uint)(SSPI == null ? 0 : SSPI.Length);

            if (FeatureExt != null)
            {
                MemoryStream featureExtension = new MemoryStream();
                FeatureExt.Pack(featureExtension);
                notFixed += (uint)(sizeof(uint) + featureExtension.Length); 
            }

            return (ushort)(FixedLength + notFixed);
        }

        /// <summary>
        /// Gets or sets the highest TDS version being used by the client
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
        /// Client host name
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Application name
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Server name
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Client library name
        /// </summary>
        public string LibraryName { get; set; }

        /// <summary>
        /// User language
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// User database
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Gets or sets Client ID
        /// </summary>
        public byte[] ClientID { get; set; }

        /// <summary>
        /// Attach database file
        /// </summary>
        public string AttachDatabaseFile { get; set; }

        /// <summary>
        /// Change password
        /// </summary>
        public string ChangePassword { get; set; }

        /// <summary>
        /// SSPI authentication blob
        /// </summary>
        public byte[] SSPI { get; set; }

        /// <summary>
        /// Feature extension in the login7
        /// </summary>
        public TDSLogin7FeatureOptionsToken FeatureExt { get; set; }

        /// <summary>
        /// Gets or sets the variable portion of this message. A stream of bytes in the order shown, indicates the offset
        /// (from the start of the message) and length of various parameters
        /// </summary>
        public List<TDSLogin7Option> Options { get; set; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as TDSLogin7PacketData);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object
        /// </summary>
        /// <param name="other">The object to compare with the current object</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public bool Equals(TDSLogin7PacketData other)
        {
            return other != null &&
                   Length() == other.Length() &&
                   TDSVersion == other.TDSVersion &&
                   PacketSize == other.PacketSize &&
                   ClientProgVer == other.ClientProgVer &&
                   ClientPID == other.ClientPID &&
                   ConnectionID == other.ConnectionID &&
                   OptionFlags1.Equals(other.OptionFlags1) &&
                   OptionFlags2.Equals(other.OptionFlags2) &&
                   TypeFlags.Equals(other.TypeFlags) &&
                   OptionFlags3.Equals(other.OptionFlags3) &&
                   ClientTimeZone == other.ClientTimeZone &&
                   ClientLCID == other.ClientLCID &&
                   ClientID.SequenceEqual(other.ClientID) &&
                   Options.All(other.Options.Contains);
        }

        // <summary>
        // Used to pack IPackageable to a stream
        // </summary>
        // <param name="stream">MemoryStream in which IPackageable is packet into</param>
        public void Pack(MemoryStream destination)
        {
            MemoryStream featureExtension = null;

            if (FeatureExt != null)
            {
                featureExtension = new MemoryStream();
                FeatureExt.Pack(featureExtension);
            }

            LittleEndianUtilities.WriteUInt(destination, Length());
            LittleEndianUtilities.WriteUInt(destination, TDSVersion);
            LittleEndianUtilities.WriteUInt(destination, PacketSize);
            LittleEndianUtilities.WriteUInt(destination, ClientProgVer);
            LittleEndianUtilities.WriteUInt(destination, ClientPID);
            LittleEndianUtilities.WriteUInt(destination, ConnectionID);

            OptionFlags1.Pack(destination);
            OptionFlags2.Pack(destination);
            TypeFlags.Pack(destination);
            OptionFlags3.Pack(destination);

            LittleEndianUtilities.WriteUInt(destination, ClientTimeZone);
            LittleEndianUtilities.WriteUInt(destination, ClientLCID);

            IList<TDSLogin7TokenOffsetProperty> variableProperties = new List<TDSLogin7TokenOffsetProperty>();

            WriteLoginDataPositionAndOffsetToStream(variableProperties, "HostName", destination, HostName);
            WriteLoginDataPositionAndOffsetToStream(variableProperties, "UserID", destination, UserID);
            WriteLoginDataPositionAndOffsetToStream(variableProperties, "Password", destination, Password);
            WriteLoginDataPositionAndOffsetToStream(variableProperties, "ApplicationName", destination, ApplicationName);
            WriteLoginDataPositionAndOffsetToStream(variableProperties, "ServerName", destination, ServerName);

            if (FeatureExt != null)
            {
                WriteLoginDataPositionAndOffsetToStream(variableProperties, "FeatureExt", destination);
            }
            else
            {
                LittleEndianUtilities.WriteUShort(destination, 0);
                LittleEndianUtilities.WriteUShort(destination, 0);
            }

            WriteLoginDataPositionAndOffsetToStream(variableProperties, "LibraryName", destination, LibraryName);
            WriteLoginDataPositionAndOffsetToStream(variableProperties, "Language", destination, Language);
            WriteLoginDataPositionAndOffsetToStream(variableProperties, "Database", destination, Database);

            ClientID ??= new byte[6];

            destination.Write(ClientID, 0, 6);

            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("SSPI"),
                                  (ushort)(variableProperties.Last().Position + variableProperties.Last().Length * 2),
                                  (ushort)(SSPI == null ? 0 : SSPI.Length)));
            LittleEndianUtilities.WriteUShort(destination, (ushort)variableProperties.Last().Position);
            LittleEndianUtilities.WriteUShort(destination, (ushort)variableProperties.Last().Length);

            WriteLoginDataPositionAndOffsetToStream(variableProperties, "AttachDatabaseFile", destination, AttachDatabaseFile);
            WriteLoginDataPositionAndOffsetToStream(variableProperties, "ChangePassword", destination, ChangePassword);

            LittleEndianUtilities.WriteUInt(destination, 0);

            WriteLoginDataPropertiesToStream(variableProperties, destination);

            featureExtension?.WriteTo(destination);
        }

        /// <summary>
        /// Helper method to write login data position and offset to stream.
        /// </summary>
        /// <param name="variableProperties"></param>
        /// <param name="propertyName"></param>
        /// <param name="destination"></param>
        /// <param name="property"></param>
        private void WriteLoginDataPositionAndOffsetToStream(IList<TDSLogin7TokenOffsetProperty> variableProperties, string propertyName, MemoryStream destination, string property = null)
        {
            ushort position;
            ushort length = (ushort)(string.IsNullOrEmpty(property) ? 0 : property.Length);
            bool isOffsetOffset = false;

            switch (propertyName)
            {
                case "HostName":
                    position = FixedLength;
                    break;
                case "FeatureExt":
                    length = sizeof(uint) / 2;
                    isOffsetOffset = true;
                    position = (ushort)(variableProperties.Last().Position + variableProperties.Last().Length * 2);
                    break;
                case "AttachDatabaseFile":
                    position = (ushort)(variableProperties.Last().Position + variableProperties.Last().Length);
                    break;
                default:
                    position = (ushort)(variableProperties.Last().Position + variableProperties.Last().Length * 2);
                    break;
            }

            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty(propertyName), position, length, isOffsetOffset));
            LittleEndianUtilities.WriteUShort(destination, (ushort)variableProperties.Last().Position);

            if (propertyName == "FeatureExt")
            {
                LittleEndianUtilities.WriteUShort(destination, (ushort)(variableProperties.Last().Length * 2));
            }
            else
            {
                LittleEndianUtilities.WriteUShort(destination, (ushort)variableProperties.Last().Length);
            }
        }

        /// <summary>
        /// Helper to write login data to stream.
        /// </summary>
        /// <param name="variableProperties"></param>
        /// <param name="destination"></param>
        private void WriteLoginDataPropertiesToStream(IList<TDSLogin7TokenOffsetProperty> properties, MemoryStream stream)
        {
            foreach (var property in properties)
            {
                if (property.Length == 0)
                    continue;

                switch (property.Property.Name)
                {
                    case "Password":
                    case "ChangePassword":
                        LittleEndianUtilities.WritePasswordString(stream, (string)property.Property.GetValue(this, null));
                        break;
                    case "FeatureExt":
                        // Property will be written at the offset immediately following all variable length data
                        property.Position = properties.Last().Position + properties.Last().Length;
                        // Write the position at which we'll be serializing the feature extension block
                        LittleEndianUtilities.WriteUInt(stream, property.Position);
                        break;
                    case "SSPI":
                        stream.Write(SSPI, 0, SSPI.Length);
                        break;
                    default:
                        LittleEndianUtilities.WriteString(stream, (string)property.Property.GetValue(this, null));
                        break;
                }
            }
        }

        /// <summary>
        /// Used to unpack IPackageable from a stream
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable</param>
        /// <returns>Returns true if successful</returns>
        public bool Unpack(MemoryStream stream)
        {
            uint length = LittleEndianUtilities.ReadUInt(stream);
            TDSVersion = LittleEndianUtilities.ReadUInt(stream);
            PacketSize = LittleEndianUtilities.ReadUInt(stream);
            ClientProgVer = LittleEndianUtilities.ReadUInt(stream);
            ClientPID = LittleEndianUtilities.ReadUInt(stream);
            ConnectionID = LittleEndianUtilities.ReadUInt(stream);
            OptionFlags1.Unpack(stream);
            OptionFlags2.Unpack(stream);
            TypeFlags.Unpack(stream);
            OptionFlags3.Unpack(stream);
            ClientTimeZone = Convert.ToUInt32(LittleEndianUtilities.ReadUInt(stream));
            ClientLCID = LittleEndianUtilities.ReadUInt(stream);

            IList<TDSLogin7TokenOffsetProperty> variableProperties = new List<TDSLogin7TokenOffsetProperty>
            {
                new TDSLogin7TokenOffsetProperty(GetType().GetProperty("HostName"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)),
                new TDSLogin7TokenOffsetProperty(GetType().GetProperty("UserID"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)),
                new TDSLogin7TokenOffsetProperty(GetType().GetProperty("Password"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)),
                new TDSLogin7TokenOffsetProperty(GetType().GetProperty("ApplicationName"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)),
                new TDSLogin7TokenOffsetProperty(GetType().GetProperty("ServerName"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream))
            };

            if (OptionFlags3.Extension == TDSLogin7OptionFlags3Extension.Exists)
            {
                // Read Feature extension. Note that this is just an offset of the value, not the value itself
                variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("FeatureExt"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream), true));
            }
            else
            {
                // Skip unused
                LittleEndianUtilities.ReadUShort(stream);
                LittleEndianUtilities.ReadUShort(stream);
            }

            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("LibraryName"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)));
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("Language"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)));
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("Database"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)));

            ClientID = new byte[6];

            stream.Read(ClientID, 0, ClientID.Length);

            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("SSPI"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)));
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("AttachDatabaseFile"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)));
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("ChangePassword"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)));

            uint sspiLength = LittleEndianUtilities.ReadUInt(stream);

            // At this point we surpassed the fixed packet length
            long inflationOffset = Length();

            // Order strings in ascending order by offset
            // For the most cases this should not change the order of the options in the stream, but just in case
            variableProperties = variableProperties.OrderBy(p => p.Position).ToList();

            int iCurrentProperty = 0;

            while (iCurrentProperty < variableProperties.Count)
            {
                TDSLogin7TokenOffsetProperty property = variableProperties[iCurrentProperty];

                if (property.Length == 0)
                {
                    iCurrentProperty++;
                    continue;
                }

                while (inflationOffset < property.Position)
                {
                    stream.ReadByte();
                    inflationOffset++;
                }

                if (property.Property.Name == "Password" || property.Property.Name == "ChangePassword")
                {
                    property.Property.SetValue(this, LittleEndianUtilities.ReadPasswordString(stream, (ushort)(property.Length * 2)), null);

                    inflationOffset += property.Length * 2;
                }
                else if (property.Property.Name == "SSPI")
                {
                    if (property.Length == ushort.MaxValue)
                    {
                        if (sspiLength > 0)
                        {
                            // We don't know how to handle SSPI packets that exceed TDS packet size
                            throw new NotSupportedException("Long SSPI blobs are not supported yet");
                        }
                    }

                    sspiLength = property.Length;
                    SSPI = new byte[sspiLength];
                    stream.Read(SSPI, 0, SSPI.Length);
                    inflationOffset += sspiLength;
                }
                else if (property.Property.Name == "FeatureExt")
                {
                    // Check if this is the property or a pointer to the property
                    if (property.IsOffsetOffset)
                    {
                        // Read the actual offset of the feature extension
                        property.Position = LittleEndianUtilities.ReadUInt(stream);

                        // Mark that now we have actual value
                        property.IsOffsetOffset = false;

                        inflationOffset += sizeof(uint);

                        variableProperties = variableProperties.OrderBy(p => p.Position).ToList();

                        // Subtract position to stay on the same spot for subsequent property
                        iCurrentProperty--;
                    }
                    else
                    {
                        FeatureExt = new TDSLogin7FeatureOptionsToken();
                        FeatureExt.Unpack(stream);
                        inflationOffset += FeatureExt.Size;
                    }
                }
                else
                {
                    property.Property.SetValue(this, LittleEndianUtilities.ReadString(stream, (ushort)(property.Length * 2)), null);
                    inflationOffset += property.Length * 2;
                }

                iCurrentProperty++;
            }

            return true;
        }
    }
}