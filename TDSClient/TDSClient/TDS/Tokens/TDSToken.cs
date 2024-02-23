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
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public abstract class TDSToken : ITDSPacketData
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
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

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override abstract bool Equals(object obj);
    }
}
