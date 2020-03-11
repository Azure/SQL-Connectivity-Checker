//  ---------------------------------------------------------------------------
//  <copyright file="TDSPreLoginPacketData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.PreLogin
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using TDSClient.TDS.Client;
    using TDSClient.TDS.Header;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Utilities;

    public class TDSPreLoginPacketData : ITDSPacketData
    {
        public List<TDSPreLoginOptionToken> Options { get; private set; }

        public TDSClientVersion ClientVersion { get; private set; }

        public TDSEncryptionOption Encryption { get; private set; }

        public ulong ThreadID { get; private set; }

        public bool MARS { get; private set; }

        public TDSClientTraceID TraceID { get; private set; }

        public bool FedAuthRequired { get; private set; }

        public byte[] Nonce { get; private set; }

        public bool Terminated { get; private set; }

        public TDSPreLoginPacketData()
        {
            Options = new List<TDSPreLoginOptionToken>();
        }

        public TDSPreLoginPacketData(TDSClientVersion clientVersion)
        {
            if (clientVersion == null)
            {
                throw new ArgumentNullException();
            }

            Options = new List<TDSPreLoginOptionToken>();
            AddOption(TDSPreLoginOptionTokenType.Version, clientVersion);
        }

        public void AddOption(TDSPreLoginOptionTokenType type, Object data)
        {
            if (Terminated)
            {
                throw new InvalidOperationException();
            }

            switch (type)
            {
                case TDSPreLoginOptionTokenType.Version:
                    {
                        if (data is TDSClientVersion && ClientVersion == null)
                        {
                            ClientVersion = (TDSClientVersion)data;

                            LoggingUtilities.WriteLogVerboseOnly($" Adding PreLogin option {type.ToString()}.");
                        }
                        else
                        {
                            throw new ArgumentException();
                        }
                        break;
                    }
                case TDSPreLoginOptionTokenType.Encryption:
                    {
                        if (data is TDSEncryptionOption)
                        {
                            Encryption = (TDSEncryptionOption)data;

                            LoggingUtilities.WriteLogVerboseOnly($" Adding PreLogin option {type.ToString()} [{Encryption.ToString()}].");
                        }
                        else
                        {
                            throw new ArgumentException();
                        }
                        break;
                    }
                case TDSPreLoginOptionTokenType.FedAuthRequired | TDSPreLoginOptionTokenType.MARS:
                    {
                        if (data is bool)
                        {
                            if (type == TDSPreLoginOptionTokenType.FedAuthRequired)
                            {
                                FedAuthRequired = (bool)data;
                            }
                            else
                            {
                                MARS = (bool)data;
                            }

                            LoggingUtilities.WriteLogVerboseOnly($" Adding PreLogin option {type.ToString()} [{(bool)data}].");
                        }
                        else
                        {
                            throw new ArgumentException();
                        }
                        break;
                    }
                case TDSPreLoginOptionTokenType.ThreadID:
                    {
                        if (data is ulong)
                        {
                            ThreadID = (ulong)data;

                            LoggingUtilities.WriteLogVerboseOnly($" Adding PreLogin option {type.ToString()} [{ThreadID}].");
                        }
                        else
                        {
                            throw new ArgumentException();
                        }
                        break;
                    }
                case TDSPreLoginOptionTokenType.TraceID:
                    {
                        if (data is TDSClientTraceID)
                        {
                            TraceID = (TDSClientTraceID)data;

                            LoggingUtilities.WriteLogVerboseOnly($" Adding PreLogin option {type.ToString()}.");
                        }
                        else
                        {
                            throw new ArgumentException();
                        }
                        break;
                    }
                case TDSPreLoginOptionTokenType.NonceOpt:
                    {
                        if (data is byte[])
                        {
                            Nonce = (byte[])data;

                            LoggingUtilities.WriteLogVerboseOnly($" Adding PreLogin option {type.ToString()}.");
                        }
                        else
                        {
                            throw new ArgumentException();
                        }
                        break;
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }

            Options.Add(new TDSPreLoginOptionToken(type));
        }

        public void Terminate()
        {
            Terminated = true;
            Options.Add(new TDSPreLoginOptionToken(TDSPreLoginOptionTokenType.Terminator));

            LoggingUtilities.WriteLogVerboseOnly($" Terminating PreLogin message.");
        }

        public void Pack(MemoryStream stream)
        {
            if (Options.Count == 0 || Options[0].Type != TDSPreLoginOptionTokenType.Version || Options[Options.Count - 1].Type != TDSPreLoginOptionTokenType.Terminator || !Terminated)
            {
                throw new InvalidOperationException();
            }

            var offset = (ushort)((Options.Count - 1) * (2 * sizeof(ushort) + sizeof(byte)) + sizeof(byte));

            foreach (var option in Options)
            {
                // ToDo
                option.Offset = offset;
                option.Pack(stream);
            }

            foreach (var option in Options)
            {
                switch (option.Type)
                {
                    case TDSPreLoginOptionTokenType.Encryption:
                        {
                            stream.WriteByte((byte)Encryption);
                            break;
                        }
                    case TDSPreLoginOptionTokenType.FedAuthRequired:
                        {
                            if (FedAuthRequired)
                            {
                                stream.WriteByte(0x01);
                            }
                            else
                            {
                                stream.WriteByte(0x00);
                            }
                            break;
                        }
                    case TDSPreLoginOptionTokenType.InstOpt:
                        {
                            throw new NotSupportedException();
                        }
                    case TDSPreLoginOptionTokenType.MARS:
                        {
                            if (MARS)
                            {
                                stream.WriteByte(0x01);
                            }
                            else
                            {
                                stream.WriteByte(0x00);
                            }
                            break;
                        }
                    case TDSPreLoginOptionTokenType.NonceOpt:
                        {
                            BigEndianUtilities.WriteByteArray(stream, Nonce);
                            break;
                        }
                    case TDSPreLoginOptionTokenType.ThreadID:
                        {
                            BigEndianUtilities.WriteULong(stream, ThreadID);
                            break;
                        }
                    case TDSPreLoginOptionTokenType.TraceID:
                        {
                            TraceID.Pack(stream);
                            break;
                        }
                    case TDSPreLoginOptionTokenType.Version:
                        {
                            ClientVersion.Pack(stream);
                            break;
                        }
                }
            }
        }

        public bool Unpack(MemoryStream stream)
        {
            {
                TDSPreLoginOptionToken option;
                do
                {
                    option = new TDSPreLoginOptionToken();
                    option.Unpack(stream);
                    Options.Add(option);
                } 
                while (option.Type != TDSPreLoginOptionTokenType.Terminator);
            }
            foreach (var option in Options)
            {
                switch (option.Type)
                {
                    case TDSPreLoginOptionTokenType.Encryption:
                        {
                            Encryption = (TDSEncryptionOption)stream.ReadByte();
                            break;
                        }
                    case TDSPreLoginOptionTokenType.FedAuthRequired:
                        {
                            FedAuthRequired = stream.ReadByte() == 1;
                            break;
                        }
                    case TDSPreLoginOptionTokenType.InstOpt:
                        {
                            throw new NotSupportedException();
                        }
                    case TDSPreLoginOptionTokenType.MARS:
                        {
                            MARS = stream.ReadByte() == 1;
                            break;
                        }
                    case TDSPreLoginOptionTokenType.NonceOpt:
                        {
                            Nonce = BigEndianUtilities.ReadByteArray(stream, 32);
                            break;
                        }
                    case TDSPreLoginOptionTokenType.ThreadID:
                        {
                            ThreadID = BigEndianUtilities.ReadULong(stream);
                            break;
                        }
                    case TDSPreLoginOptionTokenType.TraceID:
                        {
                            TraceID = new TDSClientTraceID();
                            TraceID.Unpack(stream);
                            break;
                        }
                    case TDSPreLoginOptionTokenType.Version:
                        {
                            ClientVersion = new TDSClientVersion();
                            ClientVersion.Unpack(stream);
                            break;
                        }
                    case TDSPreLoginOptionTokenType.Terminator:
                        {
                            break;
                        }
                }
            }
            Terminated = true;
            return true;
        }

        public ushort Length()
        {
            return (ushort)((Options.Count - 1) * (2 * sizeof(ushort) + sizeof(byte)) + sizeof(byte) + Options.Sum(opt => opt.Length));
        }
    }
}
