//  ---------------------------------------------------------------------------
//  <copyright file="TDSMessageType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Header
{
    /// <summary>
    /// Enum describing TDS Message Type
    /// </summary>
    public enum TDSMessageType : byte
    {
        /// <summary>
        /// SQL Batch Message
        /// </summary>
        SQLBatch = 1,

        /// <summary>
        /// TDS7 Pre Login Message
        /// </summary>
        PreTDS7Login,

        /// <summary>
        ///  RPC Message
        /// </summary>
        RPC,

        /// <summary>
        /// Tabular Result Message
        /// </summary>
        TabularResult,

        /// <summary>
        /// Attention Signal Message
        /// </summary>
        AttentionSignal = 6,

        /// <summary>
        /// Bulk Load Data Message
        /// </summary>
        BulkLoadData,

        /// <summary>
        /// Federated Authentication Token Message
        /// </summary>
        FedAuthToken,

        /// <summary>
        /// Transaction Manager Request Message
        /// </summary>
        TransactionManagerRequest = 14,

        /// <summary>
        /// TDS7 Login Message
        /// </summary>
        TDS7Login = 16,

        /// <summary>
        /// SSPI Message
        /// </summary>
        SSPI,

        /// <summary>
        /// PreLogin Message
        /// </summary>
        PreLogin
    }
}