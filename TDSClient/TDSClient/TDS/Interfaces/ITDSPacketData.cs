//  ---------------------------------------------------------------------------
//  <copyright file="ITDSPacketData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Interfaces
{
    /// <summary>
    /// Interface used to describe TDS Packet Data.
    /// </summary>
    public interface ITDSPacketData : IPackageable
    {
        /// <summary>
        /// TDS Packet Data Length.
        /// </summary>
        /// <returns>Returns TDS Packet Data portion length.</returns>
        ushort Length();
    }
}
