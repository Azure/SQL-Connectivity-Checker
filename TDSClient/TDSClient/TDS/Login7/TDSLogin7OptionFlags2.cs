//  ---------------------------------------------------------------------------
//  <copyright file="TDSLogin7OptionFlags2.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Login7
{
    using System;
    using System.IO;
    using TDSClient.TDS.Interfaces;

    /// <summary>
    /// Enum describing Language flag
    /// Set if the change to initial language needs to succeed if the connect is to
    /// succeed.
    /// </summary>
    public enum TDSLogin7OptionFlags2Language : byte
    {
        /// <summary>
        /// Init Lang Flag Warning
        /// </summary>
        InitLangWarn,

        /// <summary>
        /// Init Lang FLag Fatal
        /// </summary>
        InitLangFatal
    }

    /// <summary>
    /// Enum describing ODBC flag
    /// Set if the client is the ODBC driver. This causes the server to set ANSI_DEFAULTS
    /// to ON, CURSOR_CLOSE_ON_COMMIT and IMPLICIT_TRANSACTIONS to OFF, TEXTSIZE to
    /// 0x7FFFFFFF (2GB) (TDS 7.2 and earlier), TEXTSIZE to infinite(introduced in TDS 7.3), and
    /// ROWCOUNT to infinite.
    /// </summary>
    public enum TDSLogin7OptionFlags2ODBC : byte
    {
        /// <summary>
        /// ODBC Flag Off
        /// </summary>
        OdbcOff,

        /// <summary>
        /// ODBC Flag On
        /// </summary>
        OdbcOn
    }

    /// <summary>
    /// The type of user connecting to the server.
    /// </summary>
    public enum TDSLogin7OptionFlags2UserType : byte
    {
        /// <summary>
        /// User Type Normal
        /// </summary>
        UserNormal,

        /// <summary>
        /// User Type Server
        /// </summary>
        UserServer,

        /// <summary>
        /// User Type Distributed Query User
        /// </summary>
        UserRemuser,

        /// <summary>
        /// User Type Replication User
        /// </summary>
        UserSQLRepl
    }

    /// <summary>
    /// The type of security required by the client.
    /// </summary>
    public enum TDSLogin7OptionFlags2IntSecurity : byte
    {
        /// <summary>
        /// Integrated Security Off
        /// </summary>
        IntegratedSecurityOff,

        /// <summary>
        /// Integrated Security On
        /// </summary>
        IntegratedSecurityOn
    }

    /// <summary>
    /// TDS Login7 Message Option Flags 2
    /// </summary>
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class TDSLogin7OptionFlags2 : IPackageable, IEquatable<TDSLogin7OptionFlags2>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        /// <summary>
        /// Gets or sets the Language Flag.
        /// Set if the change to initial language needs to succeed if the connect is to
        /// succeed.
        /// </summary>
        public TDSLogin7OptionFlags2Language Language { get; set; }

        /// <summary>
        /// Gets or sets the ODBC Flag.
        /// Set if the client is the ODBC driver
        /// </summary>
        public TDSLogin7OptionFlags2ODBC ODBC { get; set; }

        /// <summary>
        /// Gets or sets the UserType Flag.
        /// The type of user connecting to the server.
        /// </summary>
        public TDSLogin7OptionFlags2UserType UserType { get; set; }

        /// <summary>
        /// Gets or sets the IntSecurity Flag.
        /// The type of security required by the client.
        /// </summary>
        public TDSLogin7OptionFlags2IntSecurity IntSecurity { get; set; }

        /// <summary>
        /// Default construgtor
        /// </summary>
        public TDSLogin7OptionFlags2()
        {
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        public TDSLogin7OptionFlags2(byte flags)
        {
            // Parse bytes as per TDS specification, section 2.2.6.3 LOGIN 7
            Language = (TDSLogin7OptionFlags2Language)(flags & 0x1);
            ODBC = (TDSLogin7OptionFlags2ODBC)((flags >> 1) & 0x1);
            // Skipping deprecated fTranBoundary and fCacheConnect
            UserType = (TDSLogin7OptionFlags2UserType)((flags >> 4) & 0x7);
            IntSecurity = (TDSLogin7OptionFlags2IntSecurity)((flags >> 7) & 0x1);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as TDSLogin7OptionFlags2);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public bool Equals(TDSLogin7OptionFlags2 other)
        {
            return other != null &&
                   this.Language == other.Language &&
                   this.ODBC == other.ODBC &&
                   this.UserType == other.UserType &&
                   this.IntSecurity == other.IntSecurity;
        }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public void Pack(MemoryStream stream)
        {
            byte packedByte = (byte)((byte)this.Language
                | ((byte)this.ODBC << 1)
                | ((byte)this.UserType << 4)
                | ((byte)this.IntSecurity << 7));

            stream.WriteByte(packedByte);
        }

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
        public bool Unpack(MemoryStream stream)
        {
            byte flagByte = Convert.ToByte(stream.ReadByte());
            this.Language = (TDSLogin7OptionFlags2Language)(flagByte & 0x01);
            this.ODBC = (TDSLogin7OptionFlags2ODBC)((flagByte >> 1) & 0x01);
            this.UserType = (TDSLogin7OptionFlags2UserType)((flagByte >> 4) & 0x07);
            this.IntSecurity = (TDSLogin7OptionFlags2IntSecurity)((flagByte >> 7) & 0x01);
            
            return true;
        }
    }
}
