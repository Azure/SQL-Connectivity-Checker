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
    using TDSClient.TDS.FedAuthMessage;

    /// <summary>
    /// Class that implements TDS communication.
    /// </summary>
    public class TDSCommunicator
    {
        /// <summary>
        /// Inner TDS Stream used for communication
        /// </summary>
        private readonly TDSStream InnerTdsStream;

        /// <summary>
        /// Inner Stream (TDS/TLS) used for communication
        /// </summary>
        private readonly Stream InnerStream;

        /// <summary>
        /// TDS packet size
        /// </summary>
        private readonly ushort PacketSize;

        /// <summary>
        /// Current TDS Communicator State
        /// </summary>
        public TDSCommunicatorState CommunicatorState;

        /// <summary>
        /// Authentication Type
        /// </summary>
        private readonly string AuthenticationType;

        /// <summary>
        /// Initializes a new instance of the <see cref="TDSCommunicator" /> class.
        /// </summary>
        /// <param name="stream">NetworkStream used for communication</param>
        /// <param name="packetSize">TDS packet size</param>
        public TDSCommunicator(Stream stream, ushort packetSize, string authenticationType)
        {
            PacketSize = packetSize;
            InnerTdsStream = new TDSStream(stream, new TimeSpan(0, 0, 30), packetSize);
            InnerStream = InnerTdsStream;
            AuthenticationType = authenticationType;
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
            // if (sslPolicyErrors == SslPolicyErrors.None)
            // {
            //     LoggingUtilities.WriteLog($"   Server certificate: {certificate.Subject}");
            //     return true;
            // }

            // LoggingUtilities.WriteLog($"   Certificate error: {sslPolicyErrors}");

            // // foreach (var (element, index) in chain.ChainElements.Cast<X509ChainElement>().Select((element, index) => (element, index)))
            // // {
            // //     LoggingUtilities.WriteLog($"   Cert details:");
            // //     LoggingUtilities.WriteLog($"    issued to {element.Certificate.Subject}");
            // //     LoggingUtilities.WriteLog($"    valid from {element.Certificate.GetEffectiveDateString()} until {element.Certificate.GetExpirationDateString()}");
            // //     LoggingUtilities.WriteLog($"    issued from {element.Certificate.Issuer}");
            // // }
            // return false;

            return true;
        }

        /// <summary>
        /// Enable Transport Layer Security over TDS
        /// </summary>
        /// <param name="server">Server FQDN</param>
        /// <param name="encryptionProtocol">Encryption Protocol</param>
        public void EnableEncryption(string server, SslProtocols encryptionProtocol)
        {
            var tempStream0 = new TDSTemporaryStream(InnerTdsStream);
            LoggingUtilities.WriteLog($"  Opening a new SslStream.");
            var tempStream1 = new SslStream(tempStream0, true, ValidateServerCertificate);
            LoggingUtilities.WriteLog($"  Trying to authenticate using {encryptionProtocol}:");
            tempStream1.AuthenticateAsClient(server, new X509CertificateCollection(), encryptionProtocol, true);
            tempStream0.InnerStream = InnerTdsStream.InnerStream;
            InnerTdsStream.InnerStream = tempStream1;

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
                Array.Resize(ref resultBuffer, curOffset + PacketSize);
                curOffset += InnerStream.Read(resultBuffer, curOffset, PacketSize);
            }
            while (!InnerTdsStream.InboundMessageTerminated);

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

                case TDSCommunicatorState.SentLogin7RecordWithFederatedAuthenticationInformationRequest:
                    {
                        result = new TDSTokenStreamPacketData();
                        result.Unpack(new MemoryStream(resultBuffer));
                        break;
                    }

                case TDSCommunicatorState.SentLogin7RecordWithCompleteAuthenticationToken:
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
            HandleMessageData(data);

            var buffer = new byte[data.Length()];
            MemoryStream ms = new MemoryStream(buffer);
            data.Pack(ms);
            InnerStream.Write(buffer, 0, buffer.Length);
            
            UpdateCommunicatorState();

        }

        private void HandleMessageData(ITDSPacketData data)
        {
            switch (CommunicatorState)
            {
                case TDSCommunicatorState.Initial:
                    HandleInitialSendState(data);
                    break;

                case TDSCommunicatorState.SentInitialPreLogin:
                    HandleSentInitialPreLoginState(data);
                    break;

                case TDSCommunicatorState.SentLogin7RecordWithFederatedAuthenticationInformationRequest:
                    HandleSentLoginRecordState(data);
                    break;

                case TDSCommunicatorState.LoggedIn:
                    HandleLoggedInState();
                    break;

                default:
                    throw new InvalidOperationException();
            }

        }

        private void UpdateCommunicatorState()
        {
            switch (CommunicatorState)
            {
                case TDSCommunicatorState.Initial:
                    CommunicatorState = TDSCommunicatorState.SentInitialPreLogin;
                    break;

                case TDSCommunicatorState.SentInitialPreLogin:
                    if (AuthenticationType.Contains("Active Directory"))
                    {
                        CommunicatorState = TDSCommunicatorState.SentLogin7RecordWithFederatedAuthenticationInformationRequest;
                    }
                    else
                    {
                        CommunicatorState = TDSCommunicatorState.SentLogin7RecordWithCompleteAuthenticationToken;
                    }
                    break;

                case TDSCommunicatorState.SentLogin7RecordWithFederatedAuthenticationInformationRequest:
                    CommunicatorState = TDSCommunicatorState.SentLogin7RecordWithCompleteAuthenticationToken;
                    break;
            }
        }

        private void HandleInitialSendState(ITDSPacketData data)
        {
            if (!(data is TDSPreLoginPacketData))
            {
                throw new InvalidDataException();
            }

            InnerTdsStream.CurrentOutboundMessageType = TDSMessageType.PreLogin;
        }

        private void HandleSentInitialPreLoginState(ITDSPacketData data)
        {
            if (!(data is TDSLogin7PacketData))
            {
                throw new InvalidDataException();
            }

            InnerTdsStream.CurrentOutboundMessageType = TDSMessageType.TDS7Login;
        }

        private void HandleSentLoginRecordState(ITDSPacketData data)
        {
            if (!(data is TDSFedAuthToken))
            {
                throw new InvalidDataException();
            }

            InnerTdsStream.CurrentOutboundMessageType = TDSMessageType.FedAuthToken;
        }

        private void HandleLoggedInState()
        {
            throw new NotSupportedException();
        }
    }
}
