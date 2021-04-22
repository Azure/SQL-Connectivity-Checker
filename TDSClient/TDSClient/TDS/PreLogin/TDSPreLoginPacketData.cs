//  ---------------------------------------------------------------------------
//  <copyright file="TDSPreLoginPacketData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.PreLogin
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using TDSClient.TDS.Client;
    using TDSClient.TDS.Header;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Utilities;

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
            this.Options = new List<TDSPreLoginOptionToken>();
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

            this.Options = new List<TDSPreLoginOptionToken>();
            this.AddOption(TDSPreLoginOptionTokenType.Version, clientVersion);
        }

        /// <summary>
        /// Gets or sets TDS PreLogin Options
        /// </summary>
        public List<TDSPreLoginOptionToken> Options { get; set; }

        /// <summary>
        /// Gets or sets TDS Client Version
        /// </summary>
        public TDSClientVersion ClientVersion { get; set; }

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
        public bool FedAuthRequired { get; set; }

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
                            //LoggingUtilities.WriteLog($"    Adding PreLogin option {type}.");
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

                            LoggingUtilities.WriteLog($"  Adding PreLogin option {type} [{this.Encryption}].");
                        }
                        else
                        {
                            throw new ArgumentException();
                        }

                        break;
                    }

                case TDSPreLoginOptionTokenType.FedAuthRequired:
                    {
                        if (data is bool)
                        {
                            this.FedAuthRequired = (bool)data;

                            LoggingUtilities.WriteLog($"  Adding PreLogin option {type} [{(bool)data}].");
                        }
                        else
                        {
                            throw new ArgumentException();
                        }

                        break;
                    }

                case TDSPreLoginOptionTokenType.MARS:
                    {
                        if (data is bool)
                        {
                            this.MARS = (bool)data;

                            LoggingUtilities.WriteLog($"  Adding PreLogin option {type} [{(bool)data}].");
                        }
                        else
                        {
                            throw new ArgumentException();
                        }

                        break;
                    }

                case TDSPreLoginOptionTokenType.ThreadID:
                    {
                        if (data is uint)
                        {
                            this.ThreadID = (uint)data;

                            LoggingUtilities.WriteLog($"  Adding PreLogin option {type} [{this.ThreadID}].");
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

                            LoggingUtilities.WriteLog($"  Adding PreLogin option {type}");
                            LoggingUtilities.WriteLog($"   ConnectionID: {new Guid(this.TraceID.TraceID).ToString().ToUpper()}", writeToSummaryLog: true);
                            LoggingUtilities.WriteLog($"   ActivityID: {new Guid(this.TraceID.ActivityID).ToString().ToUpper()}");
                            LoggingUtilities.WriteLog($"   ActivitySequence: {this.TraceID.ActivitySequence}");
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

            this.Options.Add(new TDSPreLoginOptionToken(type));
        }

        /// <summary>
        /// Terminate TDS PreLogin Packet.
        /// </summary>
        public void Terminate()
        {
            this.Terminated = true;
            this.Options.Add(new TDSPreLoginOptionToken(TDSPreLoginOptionTokenType.Terminator));

            LoggingUtilities.WriteLog($"  Adding PreLogin message terminator.");
        }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public void Pack(MemoryStream stream)
        {
            if (this.Options.Count == 0 || this.Options[0].Type != TDSPreLoginOptionTokenType.Version || this.Options[this.Options.Count - 1].Type != TDSPreLoginOptionTokenType.Terminator || !this.Terminated)
            {
                throw new InvalidOperationException();
            }

            var offset = (ushort)(((this.Options.Count - 1) * ((2 * sizeof(ushort)) + sizeof(byte))) + sizeof(byte));

            foreach (var option in this.Options)
            {
                option.Offset = offset;
                offset += option.Length;
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
                            if (this.Instance != null && this.Instance.Length != 0)
                            {
                                stream.Write(this.Instance, 0, this.Instance.Length);
                            }
                            else
                            {
                                stream.WriteByte(0x00);
                            }

                            break;
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
                            BigEndianUtilities.WriteUInt(stream, this.ThreadID);
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
                    this.Options.Add(option);
                }
                while (option.Type != TDSPreLoginOptionTokenType.Terminator);
            }

            foreach (var option in this.Options)
            {
                if (option.Length == 0)
                {
                    continue;
                }

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
                            this.Instance = new byte[option.Length];
                            stream.Read(this.Instance, 0, option.Length);

                            break;
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
                            this.ThreadID = BigEndianUtilities.ReadUInt(stream);
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

        /// <summary>
        /// TDS PreLogin Packet data portion length.
        /// </summary>
        /// <returns>Returns TDS PreLogin Packet data portion length.</returns>
        public ushort Length()
        {
            return (ushort)(((this.Options.Count - 1) * ((2 * sizeof(ushort)) + sizeof(byte))) + sizeof(byte) + this.Options.Sum(opt => opt.Length));
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as TDSPreLoginPacketData);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public bool Equals(TDSPreLoginPacketData other)
        {
            return other != null &&
                   this.Options.SequenceEqual(other.Options) &&
                   this.ClientVersion.Equals(other.ClientVersion) &&
                   this.Encryption == other.Encryption &&
                   this.ThreadID == other.ThreadID &&
                   this.MARS == other.MARS &&
                   ((this.TraceID != null && this.TraceID.Equals(other.TraceID)) || (this.TraceID == other.TraceID)) &&
                   this.FedAuthRequired == other.FedAuthRequired &&
                   ((this.Nonce != null && this.Nonce.SequenceEqual(other.Nonce)) || (this.Nonce == other.Nonce)) &&
                   ((this.Instance != null && this.Instance.SequenceEqual(other.Instance)) || (this.Instance == other.Instance)) &&
                   this.Terminated == other.Terminated;
        }
    }
}
