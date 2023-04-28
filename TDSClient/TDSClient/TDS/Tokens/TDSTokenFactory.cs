//  ---------------------------------------------------------------------------
//  <copyright file="TDSTokenFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Tokens
{
    using System;
    using System.IO;
    using TDSClient.TDS.Utilities;
    using TDSClient.TDS.FedAuthInfo;

    /// <summary>
    /// Factory used to read different tokens from a stream
    /// </summary>
    public static class TDSTokenFactory
    {
        /// <summary>
        /// Reads a TDS Token from a given MemoryStream
        /// </summary>
        /// <param name="stream">Stream that contains the token</param>
        /// <returns>Returns read TDS Token</returns>
        public static TDSToken ReadTokenFromStream(MemoryStream stream)
        {
            var tokenType = (TDSTokenType)stream.ReadByte();

            LoggingUtilities.WriteLog($"  Received {tokenType} token in Login7 response.");
            switch (tokenType)
            {
                case TDSTokenType.Error:
                    {
                        var token = new TDSErrorToken();
                        token.Unpack(stream);

                        return token;
                    }

                case TDSTokenType.EnvChange:
                    {
                        var token = new TDSEnvChangeToken();
                        token.Unpack(stream);

                        return token;
                    }

                case TDSTokenType.Info:
                    {
                        var token = new TDSInfoToken();
                        token.Unpack(stream);

                        return token;
                    }

                case TDSTokenType.FedAuthInfo:
                    {
                        var token = new TDSFedAuthInfoToken();
                        token.Unpack(stream);

                        return token;
                    }

                default:
                    {
                        IgnoreToken(tokenType, stream);

                        return null;
                    }
            }
        }

        /// <summary>
        /// Skips a token within a stream based on token type
        /// </summary>
        /// <param name="tokenType">Type of the token to ignore</param>
        /// <param name="stream">Stream that contains the token</param>
        private static void IgnoreToken(TDSTokenType tokenType, MemoryStream stream)
        {
            switch (((byte)tokenType >> 4) & 0x3)
            {
                // Variable count token
                case 0:
                    {
                        throw new NotSupportedException();
                    }

                // Zero length token
                case 1:
                    {
                        return;
                    }

                // Variable length token
                case 2:
                    {
                        if (tokenType == TDSTokenType.DataClassification)
                        {
                            throw new NotSupportedException();
                        }

                        ushort length = LittleEndianUtilities.ReadUShort(stream);
                        for (int i = 0; i < length; i++)
                        {
                            stream.ReadByte();
                        }
                        
                        return;
                    }

                // Fixed length token
                case 3:
                    {
                        var bytesToRead = Math.Pow(2, ((byte)tokenType >> 2) & 0x3);
                        
                        if (tokenType == TDSTokenType.Done || tokenType == TDSTokenType.DoneInProc || tokenType == TDSTokenType.DoneProc)
                        {
                            bytesToRead = 12; // Untill support is added
                        }

                        for (int i = 0; i < bytesToRead; i++)
                        {
                            stream.ReadByte();
                        }
                        
                        return;
                    }

                default:
                    {
                        throw new InvalidOperationException();
                    }
            }
        }
    }
}
