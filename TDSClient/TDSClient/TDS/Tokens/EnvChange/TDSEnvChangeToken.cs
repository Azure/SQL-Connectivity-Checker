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
    using System.Text;
    using TDSClient.TDS.Tokens.EnvChange;
    using TDSClient.TDS.Utilities;

    /// <summary>
    /// Class describing TDS EnvChange Token
    /// </summary>
    public class TDSEnvChangeToken : TDSToken
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TDSEnvChangeToken" /> class.
        /// </summary>
        public TDSEnvChangeToken()
        {
            this.Values = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets TDS EnvChange Token Type
        /// </summary>
        public TDSEnvChangeType Type { get; private set; }

        /// <summary>
        /// Gets TDS EnvChange Token values
        /// </summary>
        public Dictionary<string, string> Values { get; private set; }

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
            this.Type = (TDSEnvChangeType)stream.ReadByte();
            switch (this.Type)
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

                    this.Values["ProtocolProperty"] = string.Format("{0}", protocolProperty);
                    this.Values["AlternateServer"] = Encoding.Unicode.GetString(temp);

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
