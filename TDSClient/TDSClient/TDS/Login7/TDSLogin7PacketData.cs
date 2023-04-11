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
        /// Gets the fixed length of the LOGIN7 structure. 
        /// </summary>

        //TODO: check if this is correct for both scenarios
        public ushort FixedLength
        {
            get
            {
                return
                  sizeof(uint) // Length
                  + sizeof(uint) // TDSVersion
                  + sizeof(uint) // PacketSize
                  + sizeof(uint) // ClientProgramVersion
                  + sizeof(uint) // ClientPID
                  + sizeof(uint) // ConnectionID
                  + sizeof(byte) // OptionFlags1
                  + sizeof(byte) // OptionFlags2
                  + sizeof(byte) // TypeFlags
                  + sizeof(byte) // OptionFlags3
                  + sizeof(long) //ClientTimeZone
                  + sizeof(uint) //ClientLCID
                  + 2 * sizeof(ushort) // HostName
                  + 2 * sizeof(ushort) // UserName
                  + 2 * sizeof(ushort) // Password
                  + 2 * sizeof(ushort) // AppName
                  + 2 * sizeof(ushort) // ServerName
                  + 2 * sizeof(ushort) // Unused / Extension
                  + 2 * sizeof(ushort) // CltIntname
                  + 2 * sizeof(ushort) // Language
                  + 2 * sizeof(ushort) // Database
                  + 6 * sizeof(byte) // ClientID
                  + 2 * sizeof(ushort) // SSPI
                  + 2 * sizeof(ushort) // AtchDBFile
                  + 2 * sizeof(ushort) // ChangePassword
                  + sizeof(uint) // SSPILong
                  ;
            }
        }

        public ushort Length() {
            return FixedLength;
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
        /// Feature extension in the login7.
        /// </summary>
        public TDSLogin7FeatureOptionsToken FeatureExt { get; set; }

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
                   this.FixedLength == other.FixedLength &&
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

            // Calculate total length by adding strings
            uint totalPacketLength = (uint)(FixedLength
                + (uint)(string.IsNullOrEmpty(HostName) ? 0 : HostName.Length * 2)  // HostName
                + (uint)(string.IsNullOrEmpty(UserID) ? 0 : UserID.Length * 2)  // UserID
                + (uint)(string.IsNullOrEmpty(Password) ? 0 : Password.Length * 2)  // Password
                + (uint)(string.IsNullOrEmpty(ApplicationName) ? 0 : ApplicationName.Length * 2)  // ApplicationName
                + (uint)(string.IsNullOrEmpty(ServerName) ? 0 : ServerName.Length * 2)  // ServerName
                + (uint)(string.IsNullOrEmpty(LibraryName) ? 0 : LibraryName.Length * 2)  // LibraryName
                + (uint)(string.IsNullOrEmpty(Language) ? 0 : Language.Length * 2)  // Language
                + (uint)(string.IsNullOrEmpty(Database) ? 0 : Database.Length * 2)  // Database
                + (uint)(string.IsNullOrEmpty(AttachDatabaseFile) ? 0 : AttachDatabaseFile.Length * 2)  // AttachDatabaseFile
                + (uint)(string.IsNullOrEmpty(ChangePassword) ? 0 : ChangePassword.Length * 2)  // ChangePassword
                + (uint)(SSPI == null ? 0 : SSPI.Length)  // SSPI
                + 0);  // Feature extension

            MemoryStream featureExtension = null;

            // Check if we have a feature extension
            if (FeatureExt != null)
            {
                // Allocate feature extension block
                featureExtension = new MemoryStream();

                // Serialize feature extension
                FeatureExt.Pack(featureExtension);

                // Update total lentgh
                totalPacketLength += (uint)(sizeof(uint) /* Offset of feature extension data */ + featureExtension.Length /* feature extension itself*/);
            }

            LittleEndianUtilities.WriteUInt(stream, totalPacketLength);
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

            // Prepare a collection of property values that will be set later
            IList<TDSLogin7TokenOffsetProperty> variableProperties = new List<TDSLogin7TokenOffsetProperty>();

            // Write client host name
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("HostName"), FixedLength, (ushort)(string.IsNullOrEmpty(HostName) ? 0 : HostName.Length)));
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Position);
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Length);

            // Write user name and password
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("UserID"), (ushort)(variableProperties.Last().Position + variableProperties.Last().Length * 2), (ushort)(string.IsNullOrEmpty(UserID) ? 0 : UserID.Length)));
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Position);
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Length);

            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("Password"), (ushort)(variableProperties.Last().Position + variableProperties.Last().Length * 2), (ushort)(string.IsNullOrEmpty(Password) ? 0 : Password.Length)));
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Position);
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Length);

            // Write application name
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("ApplicationName"), (ushort)(variableProperties.Last().Position + variableProperties.Last().Length * 2), (ushort)(string.IsNullOrEmpty(ApplicationName) ? 0 : ApplicationName.Length)));
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Position);
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Length);

            // Write server name
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("ServerName"), (ushort)(variableProperties.Last().Position + variableProperties.Last().Length * 2), (ushort)(string.IsNullOrEmpty(ServerName) ? 0 : ServerName.Length)));
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Position);
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Length);

            // Check if we have a feature extension block
            if (FeatureExt != null)
            {
                // Write the offset of the feature extension offset (pointer to pointer)
                variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("FeatureExt"), (ushort)(variableProperties.Last().Position + variableProperties.Last().Length * 2), sizeof(uint) / 2, true));  // Should be 4 bytes, devided by 2 because the next guy multiplies by 2
                LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Position);
                LittleEndianUtilities.WriteUShort(stream, (ushort)(variableProperties.Last().Length * 2));  // Compensate for division by 2 above
            }
            else 
            {
                // Skip unused
                LittleEndianUtilities.WriteUShort(stream, 0);
                LittleEndianUtilities.WriteUShort(stream, 0);
            }

            // Write client library name
            // We do not need to account for skipped unused bytes here because they're already accounted in fixedPacketLength
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("LibraryName"), (ushort)(variableProperties.Last().Position + variableProperties.Last().Length * 2), (ushort)(string.IsNullOrEmpty(LibraryName) ? 0 : LibraryName.Length)));
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Position);
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Length);

            // Write language
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("Language"), (ushort)(variableProperties.Last().Position + variableProperties.Last().Length * 2), (ushort)(string.IsNullOrEmpty(Language) ? 0 : Language.Length)));
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Position);
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Length);

            // Write database
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("Database"), (ushort)(variableProperties.Last().Position + variableProperties.Last().Length * 2), (ushort)(string.IsNullOrEmpty(Database) ? 0 : Database.Length)));
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Position);
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Length);

            // Check if client is defined
            if (ClientID == null)
            {
                // Allocate empty identifier
                ClientID = new byte[6];
            }

            // Write unique client identifier
            stream.Write(ClientID, 0, 6);

            // Write SSPI
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("SSPI"), (ushort)(variableProperties.Last().Position + variableProperties.Last().Length * 2), (ushort)(SSPI == null ? 0 : SSPI.Length)));
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Position);
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Length);

            // Write database file to be attached. NOTE, "variableProperties.Last().Length" without " * 2" because the preceeding buffer isn't string
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("AttachDatabaseFile"), (ushort)(variableProperties.Last().Position + variableProperties.Last().Length), (ushort)(string.IsNullOrEmpty(AttachDatabaseFile) ? 0 : AttachDatabaseFile.Length)));
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Position);
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Length);

            // Write password change
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("ChangePassword"), (ushort)(variableProperties.Last().Position + variableProperties.Last().Length * 2), (ushort)(string.IsNullOrEmpty(ChangePassword) ? 0 : ChangePassword.Length)));
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Position);
            LittleEndianUtilities.WriteUShort(stream, (ushort)variableProperties.Last().Length);

            // Skip long SSPI
            LittleEndianUtilities.WriteUInt(stream, 0);            

            // We will be changing collection as we go and serialize everything. As such we can't use foreach and iterator.
            int iCurrentProperty = 0;

            // Iterate through the collection
            while (iCurrentProperty < variableProperties.Count)
            {
                // Get current property by index
                TDSLogin7TokenOffsetProperty property = variableProperties[iCurrentProperty];

                // Check if length is positive
                if (property.Length == 0)
                {
                    // Move to the next property
                    iCurrentProperty++;
                    continue;
                }

                // Check special properties
                if (property.Property.Name == "Password" || property.Property.Name == "ChangePassword")
                {
                    // Write encrypted string value
                    LittleEndianUtilities.WritePasswordString(stream, (string)property.Property.GetValue(this, null));
                }
                else if (property.Property.Name == "FeatureExt")
                {
                    // Check if we are to serialize the offset or the actual data
                    if (property.IsOffsetOffset)
                    {
                        // Property will be written at the offset immediately following all variable length data
                        property.Position = variableProperties.Last().Position + variableProperties.Last().Length;

                        // Write the position at which we'll be serializing the feature extension block
                        LittleEndianUtilities.WriteUInt(stream, property.Position);

                        // Order strings in ascending order by offset
                        variableProperties = variableProperties.OrderBy(p => p.Position).ToList();

                        // Compensate increment to the next position in order to stay on the same
                        iCurrentProperty--;

                        // No longer offset, actual data is going to follow
                        property.IsOffsetOffset = false;
                    }
                    else
                    {
                        // Transfer deflated feature extension into the login stream
                        featureExtension.WriteTo(stream);
                    }
                }
                else if (property.Property.Name == "SSPI")
                {
                    // Write SSPI
                    stream.Write(SSPI, 0, SSPI.Length);
                }
                else
                {
                    // Write the string value
                    LittleEndianUtilities.WriteString(stream, (string)property.Property.GetValue(this, null));
                }

                // Move to the next property
                iCurrentProperty++;
            }



            TDSLogin7OptionFactory.WriteOptionsToStream(stream, this.Options, this.ClientID);
            
            this.FeatureExt.Pack(stream); //here after Options we add feature ext bytes // feature id, featuredatalen, featuredata
        }

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
        public bool Unpack(MemoryStream stream)
        {
            uint length = LittleEndianUtilities.ReadUInt(stream);
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

            // Prepare a collection of property values that will be set later
            IList<TDSLogin7TokenOffsetProperty> variableProperties = new List<TDSLogin7TokenOffsetProperty>();

            // Read client host name
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("HostName"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)));

            // Read user name and password
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("UserID"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)));
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("Password"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)));

            // Read application name
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("ApplicationName"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)));

            // Read server name
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("ServerName"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)));

            // Check if extension is used
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

            // Read client library name
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("LibraryName"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)));

            // Read language
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("Language"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)));

            // Read database
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("Database"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)));

            ClientID = new byte[6];

            // Read unique client identifier
            stream.Read(ClientID, 0, ClientID.Length);

            // Read SSPI blob
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("SSPI"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)));

            // Read database file to be attached
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("AttachDatabaseFile"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)));

            // Read password change
            variableProperties.Add(new TDSLogin7TokenOffsetProperty(GetType().GetProperty("ChangePassword"), LittleEndianUtilities.ReadUShort(stream), LittleEndianUtilities.ReadUShort(stream)));

            // Read long SSPI
            uint sspiLength = LittleEndianUtilities.ReadUInt(stream);

            // At this point we surpassed the fixed packet length
            long inflationOffset = FixedLength;

            // Order strings in ascending order by offset
            // For the most cases this should not change the order of the options in the stream, but just in case
            variableProperties = variableProperties.OrderBy(p => p.Position).ToList();

            // We can't use "foreach" because FeatureExt processing changes the collection hence we can only go index-based way
            int iCurrentProperty = 0;

            // Iterate over each property
            while (iCurrentProperty < variableProperties.Count)
            {
                // Get the property at the indexed position
                TDSLogin7TokenOffsetProperty property = variableProperties[iCurrentProperty];

                // Check if length is positive
                if (property.Length == 0)
                {
                    // Move to the next property
                    iCurrentProperty++;
                    continue;
                }

                // Ensure that current offset points to the option
                while (inflationOffset < property.Position)
                {
                    // Read the stream
                    stream.ReadByte();

                    // Advance position
                    inflationOffset++;
                }

                // Check special properties
                if (property.Property.Name == "Password" || property.Property.Name == "ChangePassword")
                {
                    // Read passwod string
                    property.Property.SetValue(this, LittleEndianUtilities.ReadPasswordString(stream, (ushort)(property.Length * 2)), null);

                    // Advance the position
                    inflationOffset += (property.Length * 2);
                }
                else if (property.Property.Name == "SSPI")
                {
                    // If cbSSPI < USHRT_MAX, then this length MUST be used for SSPI and cbSSPILong MUST be ignored.
                    // If cbSSPI == USHRT_MAX, then cbSSPILong MUST be checked.
                    if (property.Length == ushort.MaxValue)
                    {
                        // If cbSSPILong > 0, then that value MUST be used. If cbSSPILong ==0, then cbSSPI (USHRT_MAX) MUST be used.
                        if (sspiLength > 0)
                        {
                            // We don't know how to handle SSPI packets that exceed TDS packet size
                            throw new NotSupportedException("Long SSPI blobs are not supported yet");
                        }
                    }

                    // Use short length instead
                    sspiLength = property.Length;

                    // Allocate buffer for SSPI data
                    SSPI = new byte[sspiLength];

                    // Read SSPI blob
                    stream.Read(SSPI, 0, SSPI.Length);

                    // Advance the position
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

                        // Advance the position
                        inflationOffset += sizeof(uint);

                        // Re-order the collection
                        variableProperties = variableProperties.OrderBy(p => p.Position).ToList();

                        // Subtract position to stay on the same spot for subsequent property
                        iCurrentProperty--;
                    }
                    else
                    {
                        // Create a list of features.
                        FeatureExt = new TDSLogin7FeatureOptionsToken();

                        // Unpack feature extension
                        FeatureExt.Unpack(stream);

                        // Advance position by the size of the inflated token
                        inflationOffset += FeatureExt.InflationSize;
                    }
                }
                else
                {
                    // Read the string and assign it to the property of this instance
                    property.Property.SetValue(this, LittleEndianUtilities.ReadString(stream, (ushort)(property.Length * 2)), null);

                    // Advance the position
                    inflationOffset += (property.Length * 2);
                }

                // Advance to the next property
                iCurrentProperty++;
            }

            return true;

            // var result = TDSLogin7OptionFactory.ReadOptionsFromStream(stream);
            // this.ClientID = result.Item2;
            // this.Options = result.Item1;

            // return true;
        }

        // /// <summary>
        // /// TDS Login7 Data portion length.
        // /// </summary>
        // /// <returns>Returns TDS Login7 Data portion length.</returns>
        // ushort ITDSPacketData.Length()
        // {
        //     return Convert.ToUInt16(this.Length);
        // }
    }
}