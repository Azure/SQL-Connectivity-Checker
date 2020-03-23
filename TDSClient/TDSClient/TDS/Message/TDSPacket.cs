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

    public class TDSPacket : IPackageable
    {
        public TDSPacket()
        {
            this.Header = new TDSPacketHeader();
        }

        public TDSPacket(TDSPacketHeader header, ITDSPacketData data)
        {
            this.Header = header;
            this.Data = data;
        }

        public TDSPacketHeader Header { get; private set; }

        public ITDSPacketData Data { get; private set; }

        public void Pack(MemoryStream stream)
        {
            this.Header.Pack(stream);
            this.Data.Pack(stream);
        }

        public bool Unpack(MemoryStream stream)
        {
            this.Header = new TDSPacketHeader();
            this.Header.Unpack(stream);
            switch (this.Header.Type)
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
                        this.Data = new TDSPreLoginPacketData();
                        this.Data.Unpack(stream);
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
