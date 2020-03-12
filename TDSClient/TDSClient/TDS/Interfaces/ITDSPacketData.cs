//  ---------------------------------------------------------------------------
//  <copyright file="ITDSPacketData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Interfaces
{
    public interface ITDSPacketData : IPackageable
    {
        ushort Length();
    }
}
