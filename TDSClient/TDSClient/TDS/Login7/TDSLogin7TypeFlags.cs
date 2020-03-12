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
        DFLT,
        TSQL
    }

    public enum TDSLogin7TypeFlagsOLEDB
    {
        Off,
        On
    }

    public enum TDSLogin7TypeFlagsReadOnlyIntent
    {
        Off,
        On
    }

    public class TDSLogin7TypeFlags : IPackageable
    {
        /// <summary>
        ///  The type of SQL the client sends to the server.
        /// </summary>
        public TDSLogin7TypeFlagsSQLType SQLType { get; set; }

        /// <summary>
        /// Set if the client is the OLEDB driver. 
        /// </summary>
        public TDSLogin7TypeFlagsOLEDB OLEDB { get; set; }

        /// <summary>
        /// Specifies that the application intent of the
        /// connection is read-only.
        /// </summary>
        public TDSLogin7TypeFlagsReadOnlyIntent ReadOnlyIntent { get; set; }

        public void Pack(MemoryStream stream)
        {
            byte packedByte = (byte)((byte)SQLType
                | ((byte)OLEDB << 4)
                | ((byte)ReadOnlyIntent << 5));
            stream.WriteByte(packedByte);
        }

        public bool Unpack(MemoryStream stream)
        {
            byte flagByte = Convert.ToByte(stream.ReadByte());
            SQLType = (TDSLogin7TypeFlagsSQLType)(flagByte & 0x0F);
            OLEDB = (TDSLogin7TypeFlagsOLEDB)((flagByte >> 4) & 0x01);
            ReadOnlyIntent = (TDSLogin7TypeFlagsReadOnlyIntent)((flagByte >> 5) & 0x01);
            return true;
        }
    }
}
