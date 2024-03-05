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
    using System.Linq;
    
    using TDSClient.TDS.Interfaces;

    /// <summary>
    /// Class describing data portion of a TDS Token Stream
    /// </summary>
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class TDSTokenStreamPacketData : ITDSPacketData, IEquatable<TDSTokenStreamPacketData>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TDSTokenStreamPacketData"/> class.
        /// </summary>
        public TDSTokenStreamPacketData()
        {
            Tokens = new List<TDSToken>();
        }

        /// <summary>
        /// Gets or sets TDS Token Stream Token List
        /// </summary>
        public List<TDSToken> Tokens { get; set; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as TDSTokenStreamPacketData);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public bool Equals(TDSTokenStreamPacketData other)
        {
            return other != null &&
                   Tokens.SequenceEqual(other.Tokens);
        }

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
                    Tokens.Add(token);
                }
            }

            return true;
        }
    }
}
