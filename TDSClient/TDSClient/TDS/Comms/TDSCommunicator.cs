using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using TDSClient.TDS.Header;
using TDSClient.TDS.PreLogin;
using TDSClient.TDS.Login7;
using TDSClient.TDS.Tokens;
using TDSClient.TDS.Interfaces;
using TDSClient.TDS.Utilities;
using System.Security.Authentication;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

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

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            LoggingUtilities.WriteLog($"Certificate error: {sslPolicyErrors}");

            return false;
        }

        public void EnableEncryption(string Server)
        {
            var tempStream0 = new TDSTemporaryStream(InnerTdsStream);
            var tempStream1 = new SslStream(tempStream0, true, ValidateServerCertificate);

            tempStream1.AuthenticateAsClient(Server, new X509CertificateCollection(), SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls, true);

            tempStream0.InnerStream = InnerTdsStream.InnerStream;
            InnerTdsStream.InnerStream = tempStream1;

            LoggingUtilities.WriteLog($"  Cipher: {tempStream1.CipherAlgorithm} strength {tempStream1.CipherStrength}");
            LoggingUtilities.WriteLog($"  Hash: {tempStream1.HashAlgorithm} strength {tempStream1.HashStrength}");
            LoggingUtilities.WriteLog($"  Key exchange: {tempStream1.KeyExchangeAlgorithm} strength {tempStream1.KeyExchangeStrength}");
            LoggingUtilities.WriteLog($"  Protocol: {tempStream1.SslProtocol}");

            LoggingUtilities.WriteLog($"  Is authenticated: {tempStream1.IsAuthenticated}");
            LoggingUtilities.WriteLog($"  IsSigned: {tempStream1.IsSigned}");
            LoggingUtilities.WriteLog($"  Is Encrypted: {tempStream1.IsEncrypted}");

            X509Certificate localCertificate = tempStream1.LocalCertificate;
            if (tempStream1.LocalCertificate != null)
            {
                LoggingUtilities.WriteLog($"  Local cert was issued to {localCertificate.Subject} and is valid from {localCertificate.GetEffectiveDateString()} until {localCertificate.GetExpirationDateString()}.");
            }
            else
            {
                LoggingUtilities.WriteLog("  Local certificate is null.");
            }
            X509Certificate remoteCertificate = tempStream1.RemoteCertificate;
            if (tempStream1.RemoteCertificate != null)
            {
                LoggingUtilities.WriteLog($"  Remote cert was issued to {remoteCertificate.Subject} and is valid from {remoteCertificate.GetEffectiveDateString()} until {remoteCertificate.GetExpirationDateString()}.");
            }
            else
            {
                LoggingUtilities.WriteLog("  Remote certificate is null.");
            }
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
