//  ---------------------------------------------------------------------------
//  <copyright file="TDSEnvChangeToken.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    
    using TDSClient.TDS.Tokens.EnvChange;
    using TDSClient.TDS.Utilities;

    /// <summary>
    /// Class describing TDS EnvChange Token
    /// </summary>
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class TDSEnvChangeToken : TDSToken, IEquatable<TDSEnvChangeToken>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TDSEnvChangeToken" /> class.
        /// </summary>
        public TDSEnvChangeToken()
        {
            Values = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets TDS EnvChange Token Type
        /// </summary>
        public ushort TokenLength { get; set; }

        /// <summary>
        /// Gets or sets TDS EnvChange Token Type
        /// </summary>
        public TDSEnvChangeType Type { get; set; }

        /// <summary>
        /// Gets or sets TDS EnvChange Token values
        /// </summary>
        public Dictionary<string, string> Values { get; set; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as TDSEnvChangeToken);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public bool Equals(TDSEnvChangeToken other)
        {
            return other != null &&
                   this.Type == other.Type &&
                   this.Values.Count == other.Values.Count &&
                   !this.Values.Except(other.Values).Any();
        }

        /// <summary>
        /// EnvChange Token Length
        /// </summary>
        /// <returns>Returns EnvChange token length</returns>
        public override ushort Length()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">Memory stream to pack the IPackageable to.</param>
        public override void Pack(MemoryStream stream)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream containing the object that needs to be unpacked.</param>
        /// <returns>Returns true if successful.</returns>
        public override bool Unpack(MemoryStream stream)
        {
            var length = LittleEndianUtilities.ReadUShort(stream);
            Type = (TDSEnvChangeType)stream.ReadByte();
            switch (Type)
            {
                case TDSEnvChangeType.Routing:
                    var routingDataValueLength = LittleEndianUtilities.ReadUShort(stream);
                    if (routingDataValueLength == 0 || stream.ReadByte() != 0)
                    {
                        throw new InvalidOperationException();
                    }

                    var protocolProperty = LittleEndianUtilities.ReadUShort(stream);
                    if (protocolProperty == 0)
                    {
                        throw new InvalidOperationException();
                    }

                    int strLength = LittleEndianUtilities.ReadUShort(stream) * 2;

                    var temp = new byte[strLength];
                    stream.Read(temp, 0, strLength);

                    Values["ProtocolProperty"] = string.Format("{0}", protocolProperty);
                    Values["AlternateServer"] = Encoding.Unicode.GetString(temp);

                    for (int i = 0; i < length - routingDataValueLength - sizeof(byte) - sizeof(ushort); i++)
                    {
                        // Ignore oldValue
                        stream.ReadByte();
                    }

                    break;
                default:
                    {
                        for (int i = 0; i < length - sizeof(byte); i++)
                        {
                            // Ignore unsupported types
                            stream.ReadByte();
                        }
                    
                        return false;
                    }
            }

            return true;
        }
    }
}
