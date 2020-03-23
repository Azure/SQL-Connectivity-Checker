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
    public class TDSLogin7OptionFlags2 : IPackageable
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
