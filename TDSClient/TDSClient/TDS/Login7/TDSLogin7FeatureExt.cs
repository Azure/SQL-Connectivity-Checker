//  ---------------------------------------------------------------------------
//  <copyright file="TDSLogin7FeatureExt.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Login7
{
    using System;
    using System.IO;

    using TDSClient.TDS.Interfaces;

    /// <summary>
    /// TDS feature identifier
    /// </summary>
    public enum TDSFeatureID : byte
    {
        /// <summary>
        /// Session recovery
        /// </summary>
        SessionRecovery = 0x01,

        /// <summary>
        /// Federated authentication
        /// </summary>
        FederatedAuthentication = 0x02,

        /// <summary>
        /// Column encryption
        /// </summary>
        ColumnEncryption = 0x04,

        /// <summary>
        /// Global transactions
        /// </summary>
        GlobalTransactions = 0x05,

        /// <summary>
        /// Azure SQL Support
        /// </summary>
        AzureSQLSupport = 0x08,

        /// <summary>
        /// Data Classification
        /// </summary>
        DataClassification = 0x09,

        /// <summary>
        /// UTF-8 encoding support
        /// </summary>
        SupportUTF8 = 0x0A,

        /// <summary
        /// Azure SQL DNS caching
        /// </summary>
        AzureSQLDNSCaching = 0x0B,

        /// <summary>
        /// End of the list
        /// </summary>
        Terminator = 0xFF
    }

    /// <summary>
    /// TDS Login7 Message Option Flags 3
    /// </summary>
    public abstract class TDSLogin7FeatureExt : IPackageable, IEquatable<TDSLogin7FeatureExt>
    {

        /// <summary>
        /// Size of the data read during inflation operation. It is needed to properly parse the option stream.
        /// </summary>
        internal uint UnpackSize { get; set; }

        /// <summary>
        /// Feature type
        /// </summary>
        public virtual TDSFeatureID FeatureID { get; protected set; }

        public bool Equals(TDSLogin7FeatureExt other)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        //public abstract bool Equals(object obj);

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public abstract void Pack(MemoryStream stream);

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
        public abstract bool Unpack(MemoryStream stream);
    }
}