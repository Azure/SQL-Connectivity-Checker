//  ---------------------------------------------------------------------------
//  <copyright file="TDSEnvChangeType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Tokens.EnvChange
{
    /// <summary>
    /// Enum describing ENcChange Token Type
    /// </summary>
    public enum TDSEnvChangeType : byte
    {
        /// <summary>
        /// EnvChange Database Token.
        /// </summary>
        Database = 1,

        /// <summary>
        /// EnvChange Language Token.
        /// </summary>
        Language,

        /// <summary>
        /// EnvChange CharacterSet Token.
        /// </summary>
        CharacterSet,

        /// <summary>
        /// EnvChange PacketSize Token.
        /// </summary>
        PacketSize,

        /// <summary>
        /// EnvChange Unicode Data Sorting Local ID Token.
        /// </summary>
        UnicodeDataSortingLocalID,

        /// <summary>
        /// EnvChange Unicode Data Sorting Comparison Flags Token.
        /// </summary>
        UnicodeDataSortingComparisonFlags,

        /// <summary>
        /// EnvChange SQLCollation Token.
        /// </summary>
        SQLCollation,

        /// <summary>
        /// EnvChange Begin Transaction Token.
        /// </summary>
        BeginTransaction,

        /// <summary>
        /// EnvChange Commit Transaction Token.
        /// </summary>
        CommitTransaction,

        /// <summary>
        /// EnvChange Rollback Transaction Token.
        /// </summary>
        RollbackTransaction,

        /// <summary>
        /// EnvChange Enlist DTC Transaction Token.
        /// </summary>
        EnlistDTCTransaction,

        /// <summary>
        /// EnvChange Defect Transaction Token.
        /// </summary>
        DefectTransaction,

        /// <summary>
        /// EnvChange Database Mirroring Partner Token.
        /// </summary>
        DatabaseMirroringPartner,

        /// <summary>
        /// EnvChange Promote Transaction Token.
        /// </summary>
        PromoteTransaction = 15,

        /// <summary>
        /// EnvChange Transaction Manager Address Token.
        /// </summary>
        TransactionManagerAddress,

        /// <summary>
        /// EnvChange Transaction Ended Token.
        /// </summary>
        TransactionEnded,

        /// <summary>
        /// EnvChange Reset Completion Ack Token.
        /// </summary>
        ResetCompletionAck,

        /// <summary>
        /// EnvChange Send Back User Info Token.
        /// </summary>
        SendBackUserInfo,

        /// <summary>
        /// EnvChange Routing Token.
        /// </summary>
        Routing
    }
}
