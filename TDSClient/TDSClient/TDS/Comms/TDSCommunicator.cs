//  ---------------------------------------------------------------------------
//  <copyright file="TDSCommunicator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Comms
{
    using System;
    using System.IO;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using TDSClient.TDS.Header;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Login7;
    using TDSClient.TDS.PreLogin;
    using TDSClient.TDS.Tokens;
    using TDSClient.TDS.Utilities;

    public class TDSCommunicator
    {
        private readonly TDSStream innerTdsStream;
        private readonly Stream innerStream;
        private readonly ushort packetSize;
        private TDSCommunicatorState communicatorState;

        public TDSCommunicator(NetworkStream stream, ushort packetSize)
        {
            this.packetSize = packetSize;
            this.innerTdsStream = new TDSStream(stream, new TimeSpan(0, 0, 30), packetSize);
            this.innerStream = this.innerTdsStream;
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            LoggingUtilities.WriteLog($"Certificate error: {sslPolicyErrors}");

            return false;
        }

        public void EnableEncryption(string server, SslProtocols encryptionProtocol)
        {
            var tempStream0 = new TDSTemporaryStream(this.innerTdsStream);
            var tempStream1 = new SslStream(tempStream0, true, ValidateServerCertificate);

            tempStream1.AuthenticateAsClient(server, new X509CertificateCollection(), encryptionProtocol, true);

            tempStream0.InnerStream = this.innerTdsStream.InnerStream;
            this.innerTdsStream.InnerStream = tempStream1;

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
                Array.Resize(ref resultBuffer, curOffset + this.packetSize);
                curOffset += this.innerStream.Read(resultBuffer, curOffset, this.packetSize);
            }
            while (!this.innerTdsStream.InboundMessageTerminated);

            Array.Resize(ref resultBuffer, curOffset);

            ITDSPacketData result;
            switch (this.communicatorState)
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
            switch (this.communicatorState)
            {
                case TDSCommunicatorState.Initial:
                    {
                        if (!(data is TDSPreLoginPacketData))
                        {
                            throw new InvalidDataException();
                        }

                        this.innerTdsStream.CurrentOutboundMessageType = TDSMessageType.PreLogin;
                        break;
                    }

                case TDSCommunicatorState.SentInitialPreLogin:
                    {
                        if (!(data is TDSLogin7PacketData))
                        {
                            throw new InvalidDataException();
                        }

                        this.innerTdsStream.CurrentOutboundMessageType = TDSMessageType.TDS7Login;
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

            this.innerStream.Write(buffer, 0, buffer.Length);

            switch (this.communicatorState)
            {
                case TDSCommunicatorState.Initial:
                    {
                        this.communicatorState = TDSCommunicatorState.SentInitialPreLogin;
                        break;
                    }

                case TDSCommunicatorState.SentInitialPreLogin:
                    {
                        this.communicatorState = TDSCommunicatorState.SentLogin7RecordWithCompleteAuthToken;
                        break;
                    }
            }
        }
    }
}
