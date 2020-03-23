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

    public static class TDSTokenFactory
    {
        public static TDSToken ReadTokenFromStream(MemoryStream stream)
        {
            var tokenType = (TDSTokenType)stream.ReadByte();

            LoggingUtilities.WriteLogVerboseOnly($" Recieved {tokenType} token in Login7 response.");
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

                default:
                    {
                        IgnoreToken(tokenType, stream);

                        return null;
                    }
            }
        }

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
