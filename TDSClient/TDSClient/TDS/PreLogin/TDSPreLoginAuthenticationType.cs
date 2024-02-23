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
        FedAuthNotRequired = 0x00,
        FedAuthRequired = 0x01,
        Illegal = 0x02
    }
}