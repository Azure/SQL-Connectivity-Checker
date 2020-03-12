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

    public enum TDSLogin7OptionFlags2Language : byte
    {
        InitLangWarn,
        InitLangFatal
    }

    public enum TDSLogin7OptionFlags2ODBC : byte
    {
        OdbcOff,
        OdbcOn
    }

    public enum TDSLogin7OptionFlags2UserType : byte
    {
        UserNormal,
        UserServer,
        UserRemuser,
        UserSQLRepl
    }

    public enum TDSLogin7OptionFlags2IntSecurity : byte
    {
        IntegratedSecurityOff,
        IntegratedSecurityOn
    }

    public class TDSLogin7OptionFlags2 : IPackageable
    {
        /// <summary>
        /// Set if the change to initial language needs to succeed if the connect is to
        /// succeed.
        /// </summary>
        public TDSLogin7OptionFlags2Language Language { get; set; }

        /// <summary>
        /// Set if the client is the ODBC driver
        /// </summary>
        public TDSLogin7OptionFlags2ODBC ODBC { get; set; }

        /// <summary>
        /// The type of user connecting to the server.
        /// </summary>
        public TDSLogin7OptionFlags2UserType UserType { get; set; }

        /// <summary>
        /// The type of security required by the client.
        /// </summary>
        public TDSLogin7OptionFlags2IntSecurity IntSecurity { get; set; }

        public void Pack(MemoryStream stream)
        {
            byte packedByte = (byte)((byte)Language
                | ((byte)ODBC << 1)
                | ((byte)UserType << 4)
                | ((byte)IntSecurity << 7));

            stream.WriteByte(packedByte);
        }

        public bool Unpack(MemoryStream stream)
        {
            byte flagByte = Convert.ToByte(stream.ReadByte());
            Language = (TDSLogin7OptionFlags2Language)(flagByte & 0x01);
            ODBC = (TDSLogin7OptionFlags2ODBC)((flagByte >> 1) & 0x01);
            UserType = (TDSLogin7OptionFlags2UserType)((flagByte >> 4) & 0x07);
            IntSecurity = (TDSLogin7OptionFlags2IntSecurity)((flagByte >> 7) & 0x01);
            return true;
        }
    }
}
