//  ---------------------------------------------------------------------------
//  <copyright file="TDSPacket.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Message
{
    using System;
    using System.IO;
    using TDSClient.TDS.Header;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.PreLogin;

    /// <summary>
    /// Class describing a TDS packet
    /// </summary>
    public class TDSPacket : IPackageable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TDSPacket" /> class.
        /// </summary>
        public TDSPacket()
        {
            Header = new TDSPacketHeader();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TDSPacket" /> class.
        /// </summary>
        /// <param name="header">TDS Packet Header</param>
        /// <param name="data">TDS Packet Data</param>
        public TDSPacket(TDSPacketHeader header, ITDSPacketData data)
        {
            Header = header;
            Data = data;
        }

        /// <summary>
        /// Gets or sets TDS Packet Header.
        /// </summary>
        public TDSPacketHeader Header { get; set; }

        /// <summary>
        /// Gets or sets TDS Packet Data.
        /// </summary>
        public ITDSPacketData Data { get; set; }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public void Pack(MemoryStream stream)
        {
            Header.Pack(stream);
            Data.Pack(stream);
        }

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
        public bool Unpack(MemoryStream stream)
        {
            Header = new TDSPacketHeader();
            Header.Unpack(stream);
            switch (Header.Type)
            {
                case TDSMessageType.AttentionSignal:
                    {
                        throw new NotSupportedException();
                    }

                case TDSMessageType.BulkLoadData:
                    {
                        throw new NotSupportedException();
                    }

                case TDSMessageType.FedAuthToken:
                    {
                        throw new NotSupportedException();
                    }

                case TDSMessageType.PreLogin:
                    {
                        Data = new TDSPreLoginPacketData();
                        Data.Unpack(stream);
                        break;
                    }

                case TDSMessageType.PreTDS7Login:
                    {
                        throw new NotSupportedException();
                    }

                case TDSMessageType.RPC:
                    {
                        throw new NotSupportedException();
                    }

                case TDSMessageType.SQLBatch:
                    {
                        throw new NotSupportedException();
                    }

                case TDSMessageType.SSPI:
                    {
                        throw new NotSupportedException();
                    }

                case TDSMessageType.TabularResult:
                    {
                        throw new NotSupportedException();
                    }

                case TDSMessageType.TDS7Login:
                    {
                        throw new NotSupportedException();
                    }

                case TDSMessageType.TransactionManagerRequest:
                    {
                        throw new NotSupportedException();
                    }
            }

            return true;
        }
    }
}
