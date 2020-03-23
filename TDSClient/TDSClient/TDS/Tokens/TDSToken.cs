//  ---------------------------------------------------------------------------
//  <copyright file="TDSToken.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Tokens
{
    using System.IO;
    using TDSClient.TDS.Interfaces;

    /// <summary>
    /// Abstract class describing functionalities of a TDS Token
    /// </summary>
    public abstract class TDSToken : ITDSPacketData
    {
        /// <summary>
        /// TDS Token length
        /// </summary>
        /// <returns>Returns TDS Token length</returns>
        public abstract ushort Length();

        /// <summary>
        /// Used to package TDS Token into a stream
        /// </summary>
        /// <param name="stream">Stream to pack the TDS Token into</param>
        public abstract void Pack(MemoryStream stream);

        /// <summary>
        /// Used to unpack a TDS Token from a given stream
        /// </summary>
        /// <param name="stream">MemoryStream that contains the token to unpack</param>
        /// <returns>Returns true if successful</returns>
        public abstract bool Unpack(MemoryStream stream);
    }
}
