//  ---------------------------------------------------------------------------
//  <copyright file="TDSTokenStreamPacketData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using TDSClient.TDS.Interfaces;

    /// <summary>
    /// Class describing data portion of a TDS Token Stream
    /// </summary>
    public class TDSTokenStreamPacketData : ITDSPacketData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TDSTokenStreamPacketData"/> class.
        /// </summary>
        public TDSTokenStreamPacketData()
        {
            this.Tokens = new LinkedList<TDSToken>();
        }

        /// <summary>
        /// Gets or sets TDS Token Stream Token List
        /// </summary>
        public LinkedList<TDSToken> Tokens { get; set; }

        /// <summary>
        /// TDS Token Stream Packet Data Length
        /// </summary>
        /// <returns>Returns TDS Token Stream Packet data portion length</returns>
        public ushort Length()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public void Pack(MemoryStream stream)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
        public bool Unpack(MemoryStream stream)
        {
            while (stream.Length > stream.Position)
            {
                TDSToken token = TDSTokenFactory.ReadTokenFromStream(stream);
                if (token != null)
                {
                    this.Tokens.AddLast(token);
                }
            }

            return true;
        }
    }
}
