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

        public void Pack(MemoryStream stream)
        {
            byte packedByte = (byte)((byte)this.SQLType
                | ((byte)this.OLEDB << 4)
                | ((byte)this.ReadOnlyIntent << 5));
            stream.WriteByte(packedByte);
        }

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
