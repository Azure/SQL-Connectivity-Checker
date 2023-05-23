//  ---------------------------------------------------------------------------
//  <copyright file="TDSFedAuthInfoOption.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.FedAuthInfo
{
    using System.IO;

    using TDSClient.TDS.Interfaces;

    /// <summary>
    /// A single fedauth information option.
    /// </summary>
    public abstract class TDSFedAuthInfoOption : IPackageable
    {
        /// <summary>
        /// FedAuth Info Identifier.
        /// </summary>
        public abstract TDSFedAuthInfoId FedAuthInfoId { get; }

        /// <summary>
        /// Initialization Constructor.
        /// </summary>
        public TDSFedAuthInfoOption()
        {
        }

        /// <summary>
        /// Inflate the token
        /// </summary>
        public abstract bool Unpack(MemoryStream source);

        /// <summary>
        /// Deflate the token.
        /// </summary>
        public abstract void Pack(MemoryStream destination);
    }
}