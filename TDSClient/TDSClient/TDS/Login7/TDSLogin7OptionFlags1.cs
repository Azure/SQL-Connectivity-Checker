//  ---------------------------------------------------------------------------
//  <copyright file="TDSLogin7OptionFlags1.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Login7
{
    using System;
    using System.IO;
    using TDSClient.TDS.Interfaces;

    public enum TDSLogin7OptionFlags1ByteOrder : byte
    {
        OrderX86,
        Order68000
    }

    public enum TDSLogin7OptionFlags1Char : byte
    {
        CharsetASCII,
        CharsetEBCDIC
    }

    public enum TDSLogin7OptionFlags1Float : byte
    {
        FloatIEEE754,
        FloatVAX,
        ND5000
    }

    public enum TDSLogin7OptionFlags1DumpLoad : byte
    {
        DumploadOn,
        DumploadOff
    }

    public enum TDSLogin7OptionFlags1UseDB : byte
    {
        UseDBOff,
        UseDBOn
    }

    public enum TDSLogin7OptionFlags1Database : byte
    {
        InitDBWarn,
        InitDBFatal
    }

    public enum TDSLogin7OptionFlags1SetLang : byte
    {
        SetLangOff,
        SetLangOn
    }

    public class TDSLogin7OptionFlags1 : IPackageable
    {
        /// <summary>
        /// The byte order used by client for numeric and datetime data types.
        /// </summary>
        public TDSLogin7OptionFlags1ByteOrder ByteOrder { get; set; }

        /// <summary>
        ///  The character set used on the client.
        /// </summary>
        public TDSLogin7OptionFlags1Char Char { get; set; }

        /// <summary>
        /// The type of floating point representation used by the client.
        /// </summary>
        public TDSLogin7OptionFlags1Float Float { get; set; }

        /// <summary>
        /// Set is dump/load or BCP capabilities are needed by the client.
        /// </summary>
        public TDSLogin7OptionFlags1DumpLoad DumpLoad { get; set; }

        /// <summary>
        /// Set if the client requires warning messages on execution of the USE SQL
        /// statement.If this flag is not set, the server MUST NOT inform the client when the database
        /// changes, and therefore the client will be unaware of any accompanying collation changes.
        /// </summary>
        public TDSLogin7OptionFlags1UseDB UseDB { get; set; }

        /// <summary>
        /// Set if the change to initial database needs to succeed if the connection is to
        /// succeed.
        /// </summary>
        public TDSLogin7OptionFlags1Database Database { get; set; }

        /// <summary>
        /// Set if the client requires warning messages on execution of a language change
        /// statement.
        /// </summary>
        public TDSLogin7OptionFlags1SetLang SetLang { get; set; }

        public void Pack(MemoryStream stream)
        {
            byte packedByte = (byte)((byte)ByteOrder
                | ((byte)Char << 1)
                | ((byte)Float << 2)
                | ((byte)DumpLoad << 4)
                | ((byte)UseDB << 5)
                | ((byte)Database << 6)
                | ((byte)SetLang << 7));
            stream.WriteByte(packedByte);
        }

        public bool Unpack(MemoryStream stream)
        {
            byte flagByte = Convert.ToByte(stream.ReadByte());
            ByteOrder = (TDSLogin7OptionFlags1ByteOrder)(flagByte & 0x01);
            Char = (TDSLogin7OptionFlags1Char)((flagByte >> 1) & 0x01);
            Float = (TDSLogin7OptionFlags1Float)((flagByte >> 2) & 0x03);
            DumpLoad = (TDSLogin7OptionFlags1DumpLoad)((flagByte >> 4) & 0x01);
            UseDB = (TDSLogin7OptionFlags1UseDB)((flagByte >> 5) & 0x01);
            Database = (TDSLogin7OptionFlags1Database)((flagByte >> 6) & 0x01);
            SetLang = (TDSLogin7OptionFlags1SetLang)((flagByte >> 7) & 0x01);
            return true;
        }
    }
}
