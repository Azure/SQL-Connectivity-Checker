﻿//  ---------------------------------------------------------------------------
//  <copyright file="TDSPreLoginOptionTokenType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.PreLogin
{
    // +-----------------+----------------------------------------------------------------------------------+
    // | PL_OPTION_TOKEN |                                   Description                                    |
    // +-----------------+----------------------------------------------------------------------------------+
    // | VERSION         | PL_OPTION_DATA = UL_VERSION                                                      |
    // |                 | US_SUBBUILD                                                                      |
    // |                 | UL_VERSION is composed of major version (1 byte), minor version (1 byte),        |
    // |                 | and build number (2 bytes). It is represented in network byte order (bigendian). |
    // |                 | On x86 platforms, UL_VERSION is prepared as follows:                             |
    // |                 | US_BUILD = SwapBytes (VER_SQL_BUILD);                                            |
    // |                 | UL_VERSION = ( (US_BUILD <16>) / (VER_SQL_MINOR <8>) / (                         |
    // |                 | VER_SQL_MAJOR) )                                                                 |
    // |                 | SwapBytes is used to swap bytes. For example, SwapBytes(0x106A)=                 |
    // |                 | 0x6A10.                                                                          |
    // +-----------------+----------------------------------------------------------------------------------+
    // | ENCRYPTION      | PL_OPTION_DATA = B_FENCRYPTION                                                   |
    // +-----------------+----------------------------------------------------------------------------------+
    // | INSTOPT         | PL_OPTION_DATA = B_INSTVALIDITY                                                  |
    // +-----------------+----------------------------------------------------------------------------------+
    // | THREADID        | PL_OPTION_DATA = UL_THREADID                                                     |
    // |                 | This value SHOULD be empty when being sent from the server to the client.        |
    // +-----------------+----------------------------------------------------------------------------------+
    // | MARS            | PL_OPTION_DATA = B_MARS                                                          |
    // |                 |  0x00 = Off                                                                     |
    // |                 |  0x01 = On                                                                      |
    // +-----------------+----------------------------------------------------------------------------------+
    // | TRACEID         | PL_OPTION_DATA = GUID_CONNID                                                     |
    // |                 | ACTIVITY_GUID                                                                    |
    // |                 | SEQUENCE_ID                                                                      |
    // +-----------------+----------------------------------------------------------------------------------+
    // | FEDAUTHREQUIRED | PL_OPTION_DATA = B_FEDAUTHREQUIRED                                               |
    // |                 | Introduced in TDS 7.4.                                                           |
    // +-----------------+----------------------------------------------------------------------------------+
    // | NONCEOPT        | PL_OPTION_DATA = NONCE                                                           |
    // |                 | The client MUST send this option if it expects to be able to use federated       |
    // |                 | authentication with Live ID Compact Token to authenticate to the server on       |
    // |                 | this connection.                                                                 |
    // |                 | If the server understands the NONCEOPT option and the client sends the           |
    // |                 | option, the server MUST respond with its own NONCEOPT.                           |
    // +-----------------+----------------------------------------------------------------------------------+
    // | TERMINATOR      | Termination token.                                                               |
    // +-----------------+----------------------------------------------------------------------------------+
   
    /// <summary>
    /// Enum describing TDS PreLogin Option Token type
    /// </summary>
    public enum TDSPreLoginOptionTokenType : byte
    {
        /// <summary>
        /// PreLogin Version Option Token.
        /// </summary>
        Version,

        /// <summary>
        /// PreLogin Encryption Option Token.
        /// </summary>
        Encryption,

        /// <summary>
        /// PreLogin Inst Option Token.
        /// </summary>
        InstOpt,

        /// <summary>
        /// PreLogin ThreadID Option Token.
        /// </summary>
        ThreadID,

        /// <summary>
        /// PreLogin MARS Option Token.
        /// </summary>
        MARS,

        /// <summary>
        /// PreLogin TraceID Option Token.
        /// </summary>
        TraceID,

        /// <summary>
        /// PreLogin FedAuth Option Token.
        /// </summary>
        FedAuthRequired,

        /// <summary>
        /// PreLogin Nonce Option Token.
        /// </summary>
        NonceOpt,

        /// <summary>
        /// PreLogin Token Terminator.
        /// </summary>
        Terminator = 0xFF
    }
}
