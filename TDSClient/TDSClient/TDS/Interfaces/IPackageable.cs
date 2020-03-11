//  ---------------------------------------------------------------------------
//  <copyright file="IPackageable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Interfaces
{
    using System.IO;

    public interface IPackageable
    {
        void Pack(MemoryStream stream);

        bool Unpack(MemoryStream stream);
    }
}
