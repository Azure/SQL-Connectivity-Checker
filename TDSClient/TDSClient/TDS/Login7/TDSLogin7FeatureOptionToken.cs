//  ---------------------------------------------------------------------------
//  <copyright file="TDSLogin7FeatureExtFedAuth.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Login7
{
    using System.IO;
    
    using TDSClient.TDS.Interfaces;

    /// <summary>
    /// Class that defines a feature option which is delivered in the login packet FeatureExt block
    /// </summary>
    public abstract class TDSLogin7FeatureOptionToken : IPackageable
    {
        /// <summary>
        /// Gets or sets the size of the data read during inflation operation. It is needed to properly parse the option stream.
        /// </summary>
        public uint Size { get; set; }

        /// <summary>
        /// Gets the feature ID.
        /// </summary>
        public virtual TDSFeatureID FeatureID { get; protected set; }

        /// <summary>
        /// Unpacks the Feature option from the given source stream.
        /// </summary>
        /// <param name="source">The source stream.</param>
        /// <returns>True if the unpacking is successful; otherwise, false.</returns>
        public abstract bool Unpack(MemoryStream source);

        /// <summary>
        /// Packs the token into the given destination stream.
        /// </summary>
        /// <param name="destination">The destination stream.</param>
        public abstract void Pack(MemoryStream destination);
    }
}
