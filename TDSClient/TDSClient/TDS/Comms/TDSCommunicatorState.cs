//  ---------------------------------------------------------------------------
//  <copyright file="TDSCommunicatorState.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Comms
{
    /// <summary>
    /// Enum describing TDS Communicator State
    /// </summary>
    public enum TDSCommunicatorState
    {
        /// <summary>
        /// Initial Communicator State
        /// </summary>
        Initial,

        /// <summary>
        /// SentInitialPreLogin Communicator State
        /// </summary>
        SentInitialPreLogin,

        /// <summary>
        /// SentLogin7RecordWithCompleteAuthToken Communicator State
        /// </summary>
        SentLogin7RecordWithCompleteAuthToken,

        /// <summary>
        /// SentLogin7RecordWithoutAuthToken Communicator State
        /// </summary>
        SentLogin7RecordWithoutAuthToken,

        /// <summary>
        /// LoggedIn Communicator State
        /// </summary>
        LoggedIn
    }
}
