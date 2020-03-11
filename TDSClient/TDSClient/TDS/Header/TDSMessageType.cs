//  ---------------------------------------------------------------------------
//  <copyright file="TDSMessageType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Header
{
    public enum TDSMessageType : byte
    {
        SQLBatch = 1,
        PreTDS7Login,
        RPC,
        TabularResult,
        AttentionSignal = 6,
        BulkLoadData,
        FedAuthToken,
        TransactionManagerRequest = 14,
        TDS7Login = 16,
        SSPI,
        PreLogin
    }
}