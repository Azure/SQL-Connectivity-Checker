//  ---------------------------------------------------------------------------
//  <copyright file="TDSFedAuthInfoId.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Tokens.FedAuthInfoToken
{
    /// <summary>
    /// TDS Fed Auth Info Id.
    /// </summary>
    public enum TDSFedAuthInfoId
    {
        /// <summary>
        /// STS URL - A Unicode string that represents 
        /// the token endpoint URL from which to
        /// acquire a Fed Auth Token
        /// </summary>
        STSURL = 0x01,

        /// <summary>
        /// Service Principal Name
        /// </summary>
        SPN = 0x02,

        /// <summary>
        /// Invalid InfoId
        /// </summary>
        Invalid = 0xEE
    }
}
