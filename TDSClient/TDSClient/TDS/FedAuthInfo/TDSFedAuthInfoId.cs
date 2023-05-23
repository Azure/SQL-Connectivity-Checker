//  ---------------------------------------------------------------------------
//  <copyright file="TDSFedAuthInfoId.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.FedAuthInfo
{
    public enum TDSFedAuthInfoId
    {
        /// <summary>
        /// STS URL as Token Endpoint
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
