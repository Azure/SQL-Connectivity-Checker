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
        public TDSPacketHeader Header { get; private set; }

        public ITDSPacketData Data { get; private set; }

        public TDSPacket()
        {
            Header = new TDSPacketHeader();
        }

        public TDSPacket(TDSPacketHeader header, ITDSPacketData data)
        {
            Header = header;
            Data = data;
        }

        public void Pack(MemoryStream stream)
        {
            Header.Pack(stream);
            Data.Pack(stream);
        }

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
