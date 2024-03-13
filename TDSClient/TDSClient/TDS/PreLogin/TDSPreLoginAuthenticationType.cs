//  ---------------------------------------------------------------------------
//  <copyright file="TdsPreLoginFedAuthRequiredOption.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.PreLogin
{
    /// <summary>
    /// FedAuthRequired option in the prelogin packet.
    /// </summary>
    public enum TdsPreLoginFedAuthRequiredOption : byte
    {
        /// <summary>
        /// FedAuthNotRequired.
        /// </summary>
        FedAuthNotRequired = 0x00,
        /// <summary>
        /// FedAuthRequired.
        /// </summary>
        FedAuthRequired = 0x01,
        /// <summary>
        /// Illegal.
        /// </summary>
        Illegal = 0x02
    }
}
