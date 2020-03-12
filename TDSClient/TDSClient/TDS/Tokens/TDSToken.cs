//  ---------------------------------------------------------------------------
//  <copyright file="TDSToken.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Tokens
{
    using System.IO;
    using TDSClient.TDS.Interfaces;

    public abstract class TDSToken : ITDSPacketData
    {
        public abstract ushort Length();

        public abstract void Pack(MemoryStream stream);

        public abstract bool Unpack(MemoryStream stream);
    }
}
