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
    using TDSClient.TDS.Comms;
    using TDSClient.TDS.Header;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Login7;
    using TDSClient.TDS.Utilities;
    using static TDSClient.AuthenticationProvider.AuthenticationProvider;

    /// <summary>
    /// Class describing data portion of the PreLogin packet
    /// </summary>
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class TDSPreLoginPacketData : ITDSPacketData, IEquatable<TDSPreLoginPacketData>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TDSPreLoginPacketData"/> class.
        /// </summary>
        public TDSPreLoginPacketData()
        {
            Options = new List<TDSPreLoginOptionToken>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TDSPreLoginPacketData"/> class.
        /// </summary>
        /// <param name="clientVersion">TDS Client version</param>
        public TDSPreLoginPacketData(TDSClientVersion clientVersion)
        {
            if (clientVersion == null)
            {
                throw new ArgumentNullException();
            }

            Options = new List<TDSPreLoginOptionToken>();
            AddOption(TDSPreLoginOptionTokenType.Version, clientVersion);
            AddOption(TDSPreLoginOptionTokenType.Encryption,
                                    TDSEncryptionOption.EncryptOff);

            AddOption(TDSPreLoginOptionTokenType.TraceID,
                      new TDSClientTraceID(Guid.NewGuid().ToByteArray(),
                                           Guid.NewGuid().ToByteArray(),
                                           0));
        }

        /// <summary>
        /// Gets or sets TDS Client Version
        /// </summary>
        public TDSClientVersion ClientVersion { get; set; }

        /// <summary>
        /// Gets or sets TDS PreLogin Options
        /// </summary>
        public List<TDSPreLoginOptionToken> Options { get; set; }

        /// <summary>
        /// Gets or sets TDS Encryption Option
        /// </summary>
        public TDSEncryptionOption Encryption { get; set; }

        /// <summary>
        /// Gets or sets Client Thread ID
        /// </summary>
        public uint ThreadID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether MARS Option is enabled
        /// </summary>
        public bool MARS { get; set; }

        /// <summary>
        /// Gets or sets Client Trace ID
        /// </summary>
        public TDSClientTraceID TraceID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Federated Authentication is required
        /// </summary>
        public TdsPreLoginFedAuthRequiredOption FedAuthRequired { get; set; }

        /// <summary>
        /// Gets or sets Nonce
        /// </summary>
        public byte[] Nonce { get; set; }

        /// <summary>
        /// Gets or sets Instance Option (not supported)
        /// </summary>
        public byte[] Instance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether TDS PreLogin Packet is terminated or not
        /// </summary>
        public bool Terminated { get; set; }

        /// <summary>
        /// Adds PreLogin option to the PreLogin packet
        /// </summary>
        /// <param name="type">Option type</param>
        /// <param name="data">Option data</param>
        public void AddOption(TDSPreLoginOptionTokenType type, object data)
        {
            if (Terminated)
            {
                throw new InvalidOperationException();
            }

            switch (type)
            {
                case TDSPreLoginOptionTokenType.Version:
                    {
                        if (data is TDSClientVersion version && ClientVersion == null)
                        {
                            ClientVersion = version;
                        }
                        else
                        {
                            throw new ArgumentException();
                        }

                        break;
                    }

                case TDSPreLoginOptionTokenType.Encryption:
                    {
                        if (data is TDSEncryptionOption option)
                        {
                            Encryption = option;

                            LoggingUtilities.WriteLog($"  Adding PreLogin option {type} [{Encryption}].");
                        }
                        else
                        {
                            throw new ArgumentException();
                        }

                        break;
                    }

                case TDSPreLoginOptionTokenType.FedAuthRequired:
                    {
                        if (data is TdsPreLoginFedAuthRequiredOption option)
                        {
                            FedAuthRequired = option;

                            LoggingUtilities.WriteLog($"  Adding PreLogin option {type} [{FedAuthRequired}].");
                        }
                        else
                        {
                            throw new ArgumentException();
                        }

                        break;
                    }

                case TDSPreLoginOptionTokenType.MARS:
                    {
                        if (data is bool boolean)
                        {
                            MARS = boolean;

                            LoggingUtilities.WriteLog($"  Adding PreLogin option {type} [{boolean}].");
                        }
                        else
                        {
                            throw new ArgumentException();
                        }

                        break;
                    }

                case TDSPreLoginOptionTokenType.ThreadID:
                    {
                        if (data is uint @int)
                        {
                            ThreadID = @int;

                            LoggingUtilities.WriteLog($"  Adding PreLogin option {type} [{ThreadID}].");
                        }
                        else
                        {
                            throw new ArgumentException();
                        }

                        break;
                    }

                case TDSPreLoginOptionTokenType.TraceID:
                    {
                        if (data is TDSClientTraceID iD)
                        {
                            TraceID = iD;

                            LoggingUtilities.WriteLog($"  Adding PreLogin option {type}");
                            LoggingUtilities.WriteLog($"   ConnectionID: {new Guid(TraceID.TraceID).ToString().ToUpper()}", writeToSummaryLog: true);
                            LoggingUtilities.WriteLog($"   ActivityID: {new Guid(TraceID.ActivityID).ToString().ToUpper()}");
                            LoggingUtilities.WriteLog($"   ActivitySequence: {TraceID.ActivitySequence}");
                        }
                        else
                        {
                            throw new ArgumentException();
                        }

                        break;
                    }

                case TDSPreLoginOptionTokenType.NonceOpt:
                    {
                        if (data is byte[] v)
                        {
                            Nonce = v;

                            LoggingUtilities.WriteLog($"  Adding PreLogin option {type}.");
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

        /// <summary>
        /// Terminate TDS PreLogin Packet.
        /// </summary>
        public void Terminate()
        {
            Terminated = true;
            Options.Add(new TDSPreLoginOptionToken(TDSPreLoginOptionTokenType.Terminator));

            LoggingUtilities.WriteLog($"  Adding PreLogin message terminator.");
        }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public void Pack(MemoryStream stream)
        {
            if (Options.Count == 0 || Options[0].Type != TDSPreLoginOptionTokenType.Version || Options[Options.Count - 1].Type != TDSPreLoginOptionTokenType.Terminator || !Terminated)
            {
                throw new InvalidOperationException();
            }

            var offset = (ushort)(((Options.Count - 1) * ((2 * sizeof(ushort)) + sizeof(byte))) + sizeof(byte));

            foreach (var option in Options)
            {
                option.Offset = offset;
                offset += option.Length;
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
                            stream.WriteByte((byte)FedAuthRequired);
                            break;
                        }

                    case TDSPreLoginOptionTokenType.InstOpt:
                        {
                            if (Instance != null && Instance.Length != 0)
                            {
                                stream.Write(Instance, 0, Instance.Length);
                            }
                            else
                            {
                                stream.WriteByte(0x00);
                            }

                            break;
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
                            BigEndianUtilities.WriteUInt(stream, ThreadID);
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

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
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
                if (option.Length == 0)
                {
                    continue;
                }

                switch (option.Type)
                {
                    case TDSPreLoginOptionTokenType.Encryption:
                        {
                            Encryption = (TDSEncryptionOption)stream.ReadByte();
                            break;
                        }

                    case TDSPreLoginOptionTokenType.FedAuthRequired:
                        {
                            FedAuthRequired = (TdsPreLoginFedAuthRequiredOption)stream.ReadByte();
                            break;
                        }

                    case TDSPreLoginOptionTokenType.InstOpt:
                        {
                            Instance = new byte[option.Length];
                            stream.Read(Instance, 0, option.Length);
                            break;
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
                            ThreadID = BigEndianUtilities.ReadUInt(stream);
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

        /// <summary>
        /// TDS PreLogin Packet data portion length.
        /// </summary>
        /// <returns>Returns TDS PreLogin Packet data portion length.</returns>
        public ushort Length()
        {
            return (ushort)(((Options.Count - 1) * ((2 * sizeof(ushort))
                  + sizeof(byte)))
                  + sizeof(byte)
                  + Options.Sum(opt => opt.Length));
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as TDSPreLoginPacketData);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public bool Equals(TDSPreLoginPacketData other)
        {
            return other != null &&
                   Options.SequenceEqual(other.Options) &&
                   ClientVersion.Equals(other.ClientVersion) &&
                   Encryption == other.Encryption &&
                   ThreadID == other.ThreadID &&
                   MARS == other.MARS &&
                   ((TraceID != null && TraceID.Equals(other.TraceID)) || (TraceID == other.TraceID)) &&
                   FedAuthRequired == other.FedAuthRequired &&
                   ((Nonce != null && Nonce.SequenceEqual(other.Nonce)) || (Nonce == other.Nonce)) &&
                   ((Instance != null && Instance.SequenceEqual(other.Instance)) || (Instance == other.Instance)) &&
                   Terminated == other.Terminated;
        }
    }
}
