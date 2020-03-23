//  ---------------------------------------------------------------------------
//  <copyright file="TDSLogin7TypeFlags.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Login7
{
    using System;
    using System.IO;
    using TDSClient.TDS.Interfaces;

    /// <summary>
    /// The type of SQL the client sends to the server.
    /// </summary>
    public enum TDSLogin7TypeFlagsSQLType
    {
        /// <summary>
        /// Default SQL Type
        /// </summary>
        DFLT,

        /// <summary>
        /// Transact-SQL Type
        /// </summary>
        TSQL
    }

    /// <summary>
    /// Enum describing OLEDB flag
    /// Set if the client is the OLEDB driver. This causes the server to set
    /// ANSI_DEFAULTS to ON, CURSOR_CLOSE_ON_COMMIT and IMPLICIT_TRANSACTIONS to
    /// OFF, TEXTSIZE to 0x7FFFFFFF (2GB) (TDS 7.2 and earlier), TEXTSIZE to infinite
    /// (introduced in TDS 7.3), and ROWCOUNT to infinite.
    /// </summary>
    public enum TDSLogin7TypeFlagsOLEDB
    {
        /// <summary>
        /// OLEDB Flag Off
        /// </summary>
        Off,

        /// <summary>
        /// OLEDB Flag On
        /// </summary>
        On
    }

    /// <summary>
    /// Enum describing ReadOnlyIntent flag
    /// This bit was introduced in TDS 7.4; however, TDS 7.1, 7.2, and 7.3
    /// clients can also use this bit in LOGIN7 to specify that the application intent of the
    /// connection is read-only.
    /// </summary>
    public enum TDSLogin7TypeFlagsReadOnlyIntent
    {
        /// <summary>
        /// Read Only Intent Flag Off
        /// </summary>
        Off,

        /// <summary>
        /// Read Only Intent Flag On
        /// </summary>
        On
    }

    /// <summary>
    /// TDS Login7 Type Flags
    /// </summary>
    public class TDSLogin7TypeFlags : IPackageable
    {
        /// <summary>
        /// Gets or sets the type of SQL the client sends to the server.
        /// </summary>
        public TDSLogin7TypeFlagsSQLType SQLType { get; set; }

        /// <summary>
        /// Gets or sets the OLEDB Flag.
        /// Set if the client is the OLEDB driver. 
        /// </summary>
        public TDSLogin7TypeFlagsOLEDB OLEDB { get; set; }

        /// <summary>
        /// Gets or sets the ReadOnlyIntent Flag.
        /// Specifies that the application intent of the
        /// connection is read-only.
        /// </summary>
        public TDSLogin7TypeFlagsReadOnlyIntent ReadOnlyIntent { get; set; }
        
        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public void Pack(MemoryStream stream)
        {
            byte packedByte = (byte)((byte)this.SQLType
                | ((byte)this.OLEDB << 4)
                | ((byte)this.ReadOnlyIntent << 5));
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
            this.SQLType = (TDSLogin7TypeFlagsSQLType)(flagByte & 0x0F);
            this.OLEDB = (TDSLogin7TypeFlagsOLEDB)((flagByte >> 4) & 0x01);
            this.ReadOnlyIntent = (TDSLogin7TypeFlagsReadOnlyIntent)((flagByte >> 5) & 0x01);
            
            return true;
        }
    }
}
