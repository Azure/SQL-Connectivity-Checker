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
        public TDSPreLoginPacketData()
        {
            this.Options = new List<TDSPreLoginOptionToken>();
        }

        public TDSPreLoginPacketData(TDSClientVersion clientVersion)
        {
            if (clientVersion == null)
            {
                throw new ArgumentNullException();
            }

            this.Options = new List<TDSPreLoginOptionToken>();
            this.AddOption(TDSPreLoginOptionTokenType.Version, clientVersion);
        }

        public List<TDSPreLoginOptionToken> Options { get; private set; }

        public TDSClientVersion ClientVersion { get; private set; }

        public TDSEncryptionOption Encryption { get; private set; }

        public ulong ThreadID { get; private set; }

        public bool MARS { get; private set; }

        public TDSClientTraceID TraceID { get; private set; }

        public bool FedAuthRequired { get; private set; }

        public byte[] Nonce { get; private set; }

        public bool Terminated { get; private set; }

        public void AddOption(TDSPreLoginOptionTokenType type, object data)
        {
            if (this.Terminated)
            {
                throw new InvalidOperationException();
            }

            switch (type)
            {
                case TDSPreLoginOptionTokenType.Version:
                    {
                        if (data is TDSClientVersion && this.ClientVersion == null)
                        {
                            this.ClientVersion = (TDSClientVersion)data;

                            LoggingUtilities.WriteLogVerboseOnly($" Adding PreLogin option {type}.");
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
                            this.Encryption = (TDSEncryptionOption)data;

                            LoggingUtilities.WriteLogVerboseOnly($" Adding PreLogin option {type} [{this.Encryption}].");
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
                                this.FedAuthRequired = (bool)data;
                            }
                            else
                            {
                                this.MARS = (bool)data;
                            }

                            LoggingUtilities.WriteLogVerboseOnly($" Adding PreLogin option {type} [{(bool)data}].");
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
                            this.ThreadID = (ulong)data;

                            LoggingUtilities.WriteLogVerboseOnly($" Adding PreLogin option {type} [{this.ThreadID}].");
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
                            this.TraceID = (TDSClientTraceID)data;

                            LoggingUtilities.WriteLogVerboseOnly($" Adding PreLogin option {type}.");
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
                            this.Nonce = (byte[])data;

                            LoggingUtilities.WriteLogVerboseOnly($" Adding PreLogin option {type}.");
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

            this.Options.Add(new TDSPreLoginOptionToken(type));
        }

        public void Terminate()
        {
            this.Terminated = true;
            this.Options.Add(new TDSPreLoginOptionToken(TDSPreLoginOptionTokenType.Terminator));

            LoggingUtilities.WriteLogVerboseOnly($" Terminating PreLogin message.");
        }

        public void Pack(MemoryStream stream)
        {
            if (this.Options.Count == 0 || this.Options[0].Type != TDSPreLoginOptionTokenType.Version || this.Options[this.Options.Count - 1].Type != TDSPreLoginOptionTokenType.Terminator || !this.Terminated)
            {
                throw new InvalidOperationException();
            }

            var offset = (ushort)(((this.Options.Count - 1) * ((2 * sizeof(ushort)) + sizeof(byte))) + sizeof(byte));

            foreach (var option in this.Options)
            {
                // ToDo
                option.Offset = offset;
                option.Pack(stream);
            }

            foreach (var option in this.Options)
            {
                switch (option.Type)
                {
                    case TDSPreLoginOptionTokenType.Encryption:
                        {
                            stream.WriteByte((byte)this.Encryption);
                            break;
                        }

                    case TDSPreLoginOptionTokenType.FedAuthRequired:
                        {
                            if (this.FedAuthRequired)
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
                            if (this.MARS)
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
                            BigEndianUtilities.WriteByteArray(stream, this.Nonce);
                            break;
                        }

                    case TDSPreLoginOptionTokenType.ThreadID:
                        {
                            BigEndianUtilities.WriteULong(stream, this.ThreadID);
                            break;
                        }

                    case TDSPreLoginOptionTokenType.TraceID:
                        {
                            this.TraceID.Pack(stream);
                            break;
                        }

                    case TDSPreLoginOptionTokenType.Version:
                        {
                            this.ClientVersion.Pack(stream);
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
                    this.Options.Add(option);
                } 
                while (option.Type != TDSPreLoginOptionTokenType.Terminator);
            }

            foreach (var option in this.Options)
            {
                switch (option.Type)
                {
                    case TDSPreLoginOptionTokenType.Encryption:
                        {
                            this.Encryption = (TDSEncryptionOption)stream.ReadByte();
                            break;
                        }

                    case TDSPreLoginOptionTokenType.FedAuthRequired:
                        {
                            this.FedAuthRequired = stream.ReadByte() == 1;
                            break;
                        }

                    case TDSPreLoginOptionTokenType.InstOpt:
                        {
                            throw new NotSupportedException();
                        }

                    case TDSPreLoginOptionTokenType.MARS:
                        {
                            this.MARS = stream.ReadByte() == 1;
                            break;
                        }

                    case TDSPreLoginOptionTokenType.NonceOpt:
                        {
                            this.Nonce = BigEndianUtilities.ReadByteArray(stream, 32);
                            break;
                        }

                    case TDSPreLoginOptionTokenType.ThreadID:
                        {
                            this.ThreadID = BigEndianUtilities.ReadULong(stream);
                            break;
                        }

                    case TDSPreLoginOptionTokenType.TraceID:
                        {
                            this.TraceID = new TDSClientTraceID();
                            this.TraceID.Unpack(stream);
                            break;
                        }
                   
                    case TDSPreLoginOptionTokenType.Version:
                        {
                            this.ClientVersion = new TDSClientVersion();
                            this.ClientVersion.Unpack(stream);
                            break;
                        }
                    
                    case TDSPreLoginOptionTokenType.Terminator:
                        {
                            break;
                        }
                }
            }

            this.Terminated = true;
            return true;
        }

        public ushort Length()
        {
            return (ushort)(((this.Options.Count - 1) * ((2 * sizeof(ushort)) + sizeof(byte))) + sizeof(byte) + this.Options.Sum(opt => opt.Length));
        }
    }
}
