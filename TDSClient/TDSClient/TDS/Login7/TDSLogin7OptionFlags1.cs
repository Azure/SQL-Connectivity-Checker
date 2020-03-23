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
        /// <summary>
        /// X86 Byte Order
        /// </summary>
        OrderX86,

        /// <summary>
        /// 68000 Byte Order
        /// </summary>
        Order68000
    }

    public enum TDSLogin7OptionFlags1Char : byte
    {
        /// <summary>
        /// ASCII Charset
        /// </summary>
        CharsetASCII,

        /// <summary>
        /// EBCDIC Charset
        /// </summary>
        CharsetEBCDIC
    }

    public enum TDSLogin7OptionFlags1Float : byte
    {
        /// <summary>
        /// IEEE754 Floating Point Representation
        /// </summary>
        FloatIEEE754,

        /// <summary>
        /// VAX Floating Point Representation
        /// </summary>
        FloatVAX,

        /// <summary>
        /// ND5000 Floating Point Representation
        /// </summary>
        ND5000
    }

    public enum TDSLogin7OptionFlags1DumpLoad : byte
    {
        /// <summary>
        /// Dump Load Flag On
        /// </summary>
        DumploadOn,

        /// <summary>
        /// Dump Load Flag Off
        /// </summary>
        DumploadOff
    }

    public enum TDSLogin7OptionFlags1UseDB : byte
    {
        /// <summary>
        /// Use DB Flag Off
        /// </summary>
        UseDBOff,

        /// <summary>
        /// Use DB Flag On
        /// </summary>
        UseDBOn
    }

    public enum TDSLogin7OptionFlags1Database : byte
    {
        /// <summary>
        /// Init DB Flag Warning
        /// </summary>
        InitDBWarn,

        /// <summary>
        /// Init DB Flag Fatal
        /// </summary>
        InitDBFatal
    }

    public enum TDSLogin7OptionFlags1SetLang : byte
    {
        /// <summary>
        /// Set Lang Flag Off
        /// </summary>
        SetLangOff,

        /// <summary>
        /// Set Lang Flag On
        /// </summary>
        SetLangOn
    }

    public class TDSLogin7OptionFlags1 : IPackageable
    {
        /// <summary>
        /// Gets or sets the byte order used by client for numeric and datetime data types.
        /// </summary>
        public TDSLogin7OptionFlags1ByteOrder ByteOrder { get; set; }

        /// <summary>
        ///  Gets or sets the character set used on the client.
        /// </summary>
        public TDSLogin7OptionFlags1Char Char { get; set; }

        /// <summary>
        /// Gets or sets the type of floating point representation used by the client.
        /// </summary>
        public TDSLogin7OptionFlags1Float Float { get; set; }

        /// <summary>
        /// Gets or sets if dump/load or BCP capabilities are needed by the client.
        /// </summary>
        public TDSLogin7OptionFlags1DumpLoad DumpLoad { get; set; }

        /// <summary>
        /// Gets or sets UseDB Flag.
        /// Set if the client requires warning messages on execution of the USE SQL
        /// statement.If this flag is not set, the server MUST NOT inform the client when the database
        /// changes, and therefore the client will be unaware of any accompanying collation changes.
        /// </summary>
        public TDSLogin7OptionFlags1UseDB UseDB { get; set; }

        /// <summary>
        /// Gets or sets the Database Flag.
        /// Set if the change to initial database needs to succeed if the connection is to
        /// succeed.
        /// </summary>
        public TDSLogin7OptionFlags1Database Database { get; set; }

        /// <summary>
        /// Gets or sets the SetLang Flag.
        /// Set if the client requires warning messages on execution of a language change
        /// statement.
        /// </summary>
        public TDSLogin7OptionFlags1SetLang SetLang { get; set; }

        public void Pack(MemoryStream stream)
        {
            byte packedByte = (byte)((byte)this.ByteOrder
                | ((byte)this.Char << 1)
                | ((byte)this.Float << 2)
                | ((byte)this.DumpLoad << 4)
                | ((byte)this.UseDB << 5)
                | ((byte)this.Database << 6)
                | ((byte)this.SetLang << 7));
            stream.WriteByte(packedByte);
        }

        public bool Unpack(MemoryStream stream)
        {
            byte flagByte = Convert.ToByte(stream.ReadByte());
            this.ByteOrder = (TDSLogin7OptionFlags1ByteOrder)(flagByte & 0x01);
            this.Char = (TDSLogin7OptionFlags1Char)((flagByte >> 1) & 0x01);
            this.Float = (TDSLogin7OptionFlags1Float)((flagByte >> 2) & 0x03);
            this.DumpLoad = (TDSLogin7OptionFlags1DumpLoad)((flagByte >> 4) & 0x01);
            this.UseDB = (TDSLogin7OptionFlags1UseDB)((flagByte >> 5) & 0x01);
            this.Database = (TDSLogin7OptionFlags1Database)((flagByte >> 6) & 0x01);
            this.SetLang = (TDSLogin7OptionFlags1SetLang)((flagByte >> 7) & 0x01);
            return true;
        }
    }
}
