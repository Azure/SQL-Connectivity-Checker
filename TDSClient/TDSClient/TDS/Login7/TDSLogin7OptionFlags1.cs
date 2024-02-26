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

    /// <summary>
    /// The byte order used by client for numeric and datetime data types.
    /// </summary>
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

    /// <summary>
    /// The character set used on the client.
    /// </summary>
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

    /// <summary>
    /// Enum describing Float flag
    /// The type of floating point representation used by the client.
    /// </summary>
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

    /// <summary>
    /// Set is dump/load or BCP capabilities are needed by the client.
    /// </summary>
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

    /// <summary>
    /// Enum describing UseDB flag
    /// Set if the client requires warning messages on execution of the USE SQL
    /// statement.If this flag is not set, the server MUST NOT inform the client when the database
    /// changes, and therefore the client will be unaware of any accompanying collation changes.
    /// </summary>
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

    /// <summary>
    /// Enum describing Database flag
    /// Set if the change to initial database needs to succeed if the connection is to
    /// succeed.
    /// </summary>
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

    /// <summary>
    /// Enum describing SetLang flag
    /// Set if the client requires warning messages on execution of a language change
    /// statement.
    /// </summary>
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

    /// <summary>
    /// TDS Login7 Message Option Flags 1
    /// </summary>
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class TDSLogin7OptionFlags1 : IPackageable, IEquatable<TDSLogin7OptionFlags1>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
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

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as TDSLogin7OptionFlags1);
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        public TDSLogin7OptionFlags1()
        {
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        public TDSLogin7OptionFlags1(byte flags)
        {
            ByteOrder = (TDSLogin7OptionFlags1ByteOrder)(flags & 0x1);
            Char = (TDSLogin7OptionFlags1Char)((flags >> 1) & 0x1);
            Float = (TDSLogin7OptionFlags1Float)((flags >> 2) & 0x3);
            DumpLoad = (TDSLogin7OptionFlags1DumpLoad)((flags >> 4) & 0x1);
            UseDB = (TDSLogin7OptionFlags1UseDB)((flags >> 5) & 0x1);
            Database = (TDSLogin7OptionFlags1Database)((flags >> 6) & 0x1);
            SetLang = (TDSLogin7OptionFlags1SetLang)((flags >> 7) & 0x1);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public bool Equals(TDSLogin7OptionFlags1 other)
        {
            return other != null &&
                   ByteOrder == other.ByteOrder &&
                   Char == other.Char &&
                   Float == other.Float &&
                   DumpLoad == other.DumpLoad &&
                   UseDB == other.UseDB &&
                   Database == other.Database &&
                   SetLang == other.SetLang;
        }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
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

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
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
