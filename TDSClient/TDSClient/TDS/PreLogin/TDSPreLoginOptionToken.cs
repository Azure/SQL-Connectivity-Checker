﻿//  ---------------------------------------------------------------------------
//  <copyright file="TDSPreLoginOptionToken.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.PreLogin
{
    using System;
    using System.IO;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Utilities;

    /// <summary>
    /// Class describing TDS PreLogin Option Token
    /// </summary>
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class TDSPreLoginOptionToken : IPackageable, IEquatable<TDSPreLoginOptionToken>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TDSPreLoginOptionToken"/> class.
        /// </summary>
        public TDSPreLoginOptionToken()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TDSPreLoginOptionToken"/> class.
        /// </summary>
        /// <param name="type">PreLogin Option Token Type</param>
        public TDSPreLoginOptionToken(TDSPreLoginOptionTokenType type)
        {
            this.Type = type;
            switch (this.Type)
            {
                case TDSPreLoginOptionTokenType.Encryption:
                    {
                        this.Length = 1;
                        break;
                    }

                case TDSPreLoginOptionTokenType.FedAuthRequired:
                    {
                        this.Length = 1;
                        break;
                    }

                case TDSPreLoginOptionTokenType.InstOpt:
                    {
                        throw new NotSupportedException();
                    }

                case TDSPreLoginOptionTokenType.MARS:
                    {
                        this.Length = 1;
                        break;
                    }

                case TDSPreLoginOptionTokenType.NonceOpt:
                    {
                        this.Length = 32;
                        break;
                    }

                case TDSPreLoginOptionTokenType.Terminator:
                    {
                        this.Length = 0;
                        break;
                    }

                case TDSPreLoginOptionTokenType.ThreadID:
                    {
                        this.Length = 4;
                        break;
                    }

                case TDSPreLoginOptionTokenType.TraceID:
                    {
                        this.Length = 36;
                        break;
                    }

                case TDSPreLoginOptionTokenType.Version:
                    {
                        this.Length = 6;
                        break;
                    }
            }
        }

        /// <summary>
        /// Gets or sets TDS PreLogin Option Token Type.
        /// </summary>
        public TDSPreLoginOptionTokenType Type { get; set; }

        /// <summary>
        /// Gets or sets Offset.
        /// </summary>
        public ushort Offset { get; set; }

        /// <summary>
        /// Gets or sets Length.
        /// </summary>
        public ushort Length { get; set; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as TDSPreLoginOptionToken);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public bool Equals(TDSPreLoginOptionToken other)
        {
            return other != null &&
                   this.Type == other.Type &&
                   this.Offset == other.Offset &&
                   this.Length == other.Length;
        }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public void Pack(MemoryStream stream)
        {
            stream.WriteByte((byte)this.Type);
            if (this.Type != TDSPreLoginOptionTokenType.Terminator)
            {
                BigEndianUtilities.WriteUShort(stream, this.Offset);
                BigEndianUtilities.WriteUShort(stream, this.Length);
            }
        }

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
        public bool Unpack(MemoryStream stream)
        {
            this.Type = (TDSPreLoginOptionTokenType)stream.ReadByte();

            if (this.Type != TDSPreLoginOptionTokenType.Terminator)
            {
                this.Offset = BigEndianUtilities.ReadUShort(stream);
                this.Length = BigEndianUtilities.ReadUShort(stream);
            }

            return true;
        }
    }
}
