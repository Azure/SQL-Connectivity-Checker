//  ---------------------------------------------------------------------------
//  <copyright file="IPackageable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Interfaces
{
    using System.IO;

    /// <summary>
    /// Interface used to describe all stream-packageable classes.
    /// </summary>
    public interface IPackageable
    {
        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        void Pack(MemoryStream stream);

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
        bool Unpack(MemoryStream stream);
    }
}
