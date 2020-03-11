//  ---------------------------------------------------------------------------
//  <copyright file="TDSEnvChangeType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Tokens.EnvChange
{
    public enum TDSEnvChangeType : byte
    {
        Database = 1,
        Language,
        CharacterSet,
        PacketSize,
        UnicodeDataSortingLocalID,
        UnicodeDataSortingComparisonFlags,
        SQLCollation,
        BeginTransaction,
        CommitTransaction,
        RollbackTransaction,
        EnlistDTCTransaction,
        DefectTransaction,
        DatabaseMirroringPartner,
        PromoteTransaction = 15,
        TransactionManagerAddress,
        TransactionEnded,
        ResetCompletionAck,
        SendBackUserInfo,
        Routing
    }
}
