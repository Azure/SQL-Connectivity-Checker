//  ---------------------------------------------------------------------------
//  <copyright file="AuthenticationType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS
{
    /// <summary>
    /// Type of authentication used.
    /// </summary>
    public enum AuthenticationType
    {
        /// <summary>
        /// SQL Authentication
        /// </summary>
        SQLAuthentication,

        /// <summary>
        /// AAD Authentication using username and password
        /// </summary>
        AADPasswordAuthentication,

        /// <summary>
        /// Integrated AAD Authentication
        /// </summary>
        AADIntegratedAuthentication,

        /// <summary>
        /// Multifactor AAD Authentication
        /// </summary>
        AADMFAAuthentication
    }
}