//  ---------------------------------------------------------------------------
//  <copyright file="TDSFedAuthInfoOption.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Tokens.FedAuthInfoToken
{
    using System.IO;

    using TDSClient.TDS.Interfaces;

    /// <summary>
    /// A single fedauth information option.
    /// </summary>
    public abstract class TDSFedAuthInfoOption : IPackageable
    {
        /// <summary>
        /// Information Data Length
        /// </summary>
        public uint InfoDataLength;

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
        /// Unpack the token
        /// </summary>
        public abstract bool Unpack(MemoryStream source);

        /// <summary>
        /// Pack the token.
        /// </summary>
        public abstract void Pack(MemoryStream destination);
    }
}