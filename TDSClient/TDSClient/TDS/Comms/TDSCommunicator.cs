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
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using TDSClient.TDS.Header;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Login7;
    using TDSClient.TDS.PreLogin;
    using TDSClient.TDS.Tokens;
    using TDSClient.TDS.Utilities;

    /// <summary>
    /// Class that implements TDS communication.
    /// </summary>
    public class TDSCommunicator
    {
        /// <summary>
        /// Inner TDS Stream used for communication
        /// </summary>
        private readonly TDSStream innerTdsStream;

        /// <summary>
        /// Inner Stream (TDS/TLS) used for communication
        /// </summary>
        private readonly Stream innerStream;

        /// <summary>
        /// TDS packet size
        /// </summary>
        private readonly ushort packetSize;

        /// <summary>
        /// Current TDS Communicator State
        /// </summary>
        private TDSCommunicatorState communicatorState;

        /// <summary>
        /// Initializes a new instance of the <see cref="TDSCommunicator" /> class.
        /// </summary>
        /// <param name="stream">NetworkStream used for communication</param>
        /// <param name="packetSize">TDS packet size</param>
        public TDSCommunicator(Stream stream, ushort packetSize)
        {
            this.packetSize = packetSize;
            this.innerTdsStream = new TDSStream(stream, new TimeSpan(0, 0, 30), packetSize);
            this.innerStream = this.innerTdsStream;
        }

        /// <summary>
        /// Validate Server Certificate
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="certificate">X509 Certificate</param>
        /// <param name="chain">X509 Chain</param>
        /// <param name="sslPolicyErrors">SSL Policy Errors</param>
        /// <returns>Returns true if no errors occurred.</returns>
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            LoggingUtilities.WriteLog($"Certificate error: {sslPolicyErrors}");

            return false;
        }

        /// <summary>
        /// Enable Transport Layer Security over TDS
        /// </summary>
        /// <param name="server">Server FQDN</param>
        /// <param name="encryptionProtocol">Encryption Protocol</param>
        public void EnableEncryption(string server, SslProtocols encryptionProtocol)
        {
            var tempStream0 = new TDSTemporaryStream(this.innerTdsStream);
            LoggingUtilities.WriteLog($"  Opening a new SslStream.");
            var tempStream1 = new SslStream(tempStream0, true, ValidateServerCertificate);
            LoggingUtilities.WriteLog($"  Trying to authenticate using {encryptionProtocol}:");
            tempStream1.AuthenticateAsClient(server, new X509CertificateCollection(), encryptionProtocol, true);
            tempStream0.InnerStream = this.innerTdsStream.InnerStream;
            this.innerTdsStream.InnerStream = tempStream1;

            LoggingUtilities.WriteLog($"   Cipher: {tempStream1.CipherAlgorithm} strength {tempStream1.CipherStrength}");
            LoggingUtilities.WriteLog($"   Hash: {tempStream1.HashAlgorithm} strength {tempStream1.HashStrength}");
            
            if ((int)tempStream1.KeyExchangeAlgorithm == 44550)
            {
                LoggingUtilities.WriteLog($"   Key exchange: ECDHE strength {tempStream1.KeyExchangeStrength}");
            }
            else
            {
                LoggingUtilities.WriteLog($"   Key exchange: {tempStream1.KeyExchangeAlgorithm} strength {tempStream1.KeyExchangeStrength}");
            }

            LoggingUtilities.WriteLog($"   Protocol: {tempStream1.SslProtocol}");

            LoggingUtilities.WriteLog($"   Is authenticated: {tempStream1.IsAuthenticated}");
            LoggingUtilities.WriteLog($"   IsSigned: {tempStream1.IsSigned}");
            LoggingUtilities.WriteLog($"   Is Encrypted: {tempStream1.IsEncrypted}");

            X509Certificate localCertificate = tempStream1.LocalCertificate;
            if (tempStream1.LocalCertificate != null)
            {
                LoggingUtilities.WriteLog($"   Local cert was issued to {localCertificate.Subject} and is valid from {localCertificate.GetEffectiveDateString()} until {localCertificate.GetExpirationDateString()}.");
            }
            else
            {
                LoggingUtilities.WriteLog("   Local certificate is null.");
            }

            X509Certificate remoteCertificate = tempStream1.RemoteCertificate;
            if (tempStream1.RemoteCertificate != null)
            {
                LoggingUtilities.WriteLog($"   Remote cert was issued to {remoteCertificate.Subject} and is valid from {remoteCertificate.GetEffectiveDateString()} until {remoteCertificate.GetExpirationDateString()}.");
            }
            else
            {
                LoggingUtilities.WriteLog("   Remote certificate is null.");
            }
        }

        /// <summary>
        /// Receive TDS Message from the server.
        /// </summary>
        /// <returns>Returns received TDS Message.</returns>
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

        /// <summary>
        /// Send TDS Message to the server.
        /// </summary>
        /// <param name="data">TDS Message Data</param>
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
