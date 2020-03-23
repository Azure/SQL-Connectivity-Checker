//  ---------------------------------------------------------------------------
//  <copyright file="TDSMessageStatus.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Header
{
    public enum TDSMessageStatus : byte
    {
        /// <summary>
        /// Normal TDS Message Status
        /// </summary>
        Normal,

        /// <summary>
        /// TDS Message Terminator
        /// The packet is the last packet in the whole request.
        /// </summary>
        EndOfMessage,

        /// <summary>
        /// IgnoreEvent TDS Message Status
        /// Ignore this event (0x01 MUST also be set).
        /// </summary>
        IgnoreEvent,

        /// <summary>
        /// ResetConnection TDS Message Status
        /// Reset this connection before processing event.
        /// </summary>
        ResetConnection = 0x08,

        /// <summary>
        /// ResetConnectionSkipTran TDS Message Status
        /// Reset the connection before processing event but do not modify the transaction
        /// state (the state will remain the same before and after the reset). 
        /// </summary>
        ResetConnectionSkipTran = 0x10
    }
}
