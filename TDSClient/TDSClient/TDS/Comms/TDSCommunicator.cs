using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using TDSClient.TDS.Header;
using TDSClient.TDS.PreLogin;
using TDSClient.TDS.Login7;
using TDSClient.TDS.Tokens;
using TDSClient.TDS.Interfaces;

namespace TDSClient.TDS.Comms
{
    public class TDSCommunicator
    {
        private readonly TDSStream InnerTdsStream;
        private readonly Stream InnerStream;
        private readonly ushort PacketSize;
        public TDSCommunicatorState CommunicatorState { get; private set; }

        public TDSCommunicator(NetworkStream stream, ushort packetSize)
        {
            PacketSize = packetSize;
            InnerTdsStream = new TDSStream(stream, new TimeSpan(0, 0, 30), packetSize);
            InnerStream = InnerTdsStream;
        }

        public void EnableEncryption(string Server)
        {
            var tempStream0 = new TDSTemporaryStream(InnerTdsStream);
            var tempStream1 = new SslStream(tempStream0, true);

            tempStream1.AuthenticateAsClient(Server);

            tempStream0.InnerStream = InnerTdsStream.InnerStream;
            InnerTdsStream.InnerStream = tempStream1;
        }

        public ITDSPacketData ReceiveTDSMessage()
        {
            byte[] resultBuffer = null;
            var curOffset = 0;

            do
            {
                Array.Resize(ref resultBuffer, curOffset + PacketSize);
                curOffset += InnerStream.Read(resultBuffer, curOffset, PacketSize);
            } while (!InnerTdsStream.InboundMessageTerminated);

            Array.Resize(ref resultBuffer, curOffset);

            ITDSPacketData result;
            switch (CommunicatorState)
            {
                case TDSCommunicatorState.SentInitialPreLogin:
                    {
                        result = new TDSPreLoginPacketData();
                        result.Unpack(new MemoryStream(resultBuffer));
                        break;
                    }
                case TDSCommunicatorState.SentLogin7RecordWithCompleteAuthToken:
                    {
                        result = new TDSTokenStreamPacketData();
                        result.Unpack(new MemoryStream(resultBuffer));
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException();
                    }
            }

            return result;
        }

        public void SendTDSMessage(ITDSPacketData data)
        {
            switch (CommunicatorState)
            {
                case TDSCommunicatorState.Initial:
                    {
                        if (!(data is TDSPreLoginPacketData))
                        {
                            throw new InvalidDataException();
                        }
                        InnerTdsStream.CurrentOutboundMessageType = TDSMessageType.PreLogin;
                        break;
                    }
                case TDSCommunicatorState.SentInitialPreLogin:
                    {
                        if (!(data is TDSLogin7PacketData))
                        {
                            throw new InvalidDataException();
                        }
                        InnerTdsStream.CurrentOutboundMessageType = TDSMessageType.TDS7Login;
                        break;
                    }
                case TDSCommunicatorState.LoggedIn:
                    {
                        throw new NotSupportedException();
                    }
                default:
                    {
                        throw new InvalidOperationException();
                    }
            }

            var buffer = new byte[data.Length()];
            data.Pack(new MemoryStream(buffer));

            InnerStream.Write(buffer, 0, buffer.Length);

            switch (CommunicatorState)
            {
                case TDSCommunicatorState.Initial:
                    {
                        CommunicatorState = TDSCommunicatorState.SentInitialPreLogin;
                        break;
                    }
                case TDSCommunicatorState.SentInitialPreLogin:
                    {
                        CommunicatorState = TDSCommunicatorState.SentLogin7RecordWithCompleteAuthToken;
                        break;
                    }
            }
        }
    }
}
