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
    /// Class that defines a a feature option which is delivered in the login packet FeatureExt block
    /// </summary>
    public abstract class TDSLogin7FeatureOptionToken : IPackageable
    {
        /// <summary>
        /// Size of the data read during inflation operation. It is needed to properly parse the option stream.
        /// </summary>
        internal uint Size { get; set; }
        
        /// <summary>
        /// Feature type
        /// </summary>
        public virtual TDSFeatureID FeatureID { get; protected set; }

        /// <summary>
        /// Unpack the Feature option
        /// </summary>
        public abstract bool Unpack(MemoryStream source);

        /// <summary>
        /// Pack the token
        /// </summary>
        public abstract void Pack(MemoryStream destination);
    }
}