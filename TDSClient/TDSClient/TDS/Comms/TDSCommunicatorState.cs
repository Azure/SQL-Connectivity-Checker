//  ---------------------------------------------------------------------------
//  <copyright file="TDSCommunicatorState.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Comms
{
    public enum TDSCommunicatorState
    {
        Initial,
        SentInitialPreLogin,
        SentLogin7RecordWithCompleteAuthToken,
        LoggedIn
    }
}
