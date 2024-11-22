//  ---------------------------------------------------------------------------
//  <copyright file="TDSSQLTestClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Client
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using TDSClient.AuthenticationProvider;
    using TDSClient.TDS.Comms;
    using TDSClient.TDS.Header;
    using TDSClient.TDS.Login7;
    using TDSClient.TDS.PreLogin;
    using TDSClient.TDS.Utilities;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.FedAuthMessage;
    using TDSClient.TDS.Tokens;
    using TDSClient.TDS.Tokens.FedAuthInfoToken;

    using static TDSClient.AuthenticationProvider.AuthenticationProvider;
    using System.Threading;

    /// <summary>
    /// SQL Test Client used to run diagnostics on SQL Server using TDS protocol.
    /// </summary>
    public class TDSSQLTestClient
    {
        private bool Reconnect;
        private int ConnectionAttempt;
        private readonly TDSAuthenticationType AuthenticationType;
        private readonly TDSAuthenticationLibrary AuthenticationLibrary;
        private string Server;
        private int Port;
        private readonly string UserID;
        private readonly string Password;
        private readonly string Database;
        private readonly string IdentityClientId;
        private TDSCommunicator TdsCommunicator;
        private TcpClient Client;
        private readonly TDSClientVersion Version;
        private readonly SslProtocols EncryptionProtocol;
        private readonly bool TrustServerCertificate;

        /// <summary>
        /// Initializes a new instance of the <see cref="TDSSQLTestClient"/> class.
        /// </summary>
        /// <param name="server">Server to connect to</param>
        /// <param name="port">Port to connect to</param>
        /// <param name="authenticationType">Type of authentication to use</param>
        /// <param name="userID">Used ID</param>
        /// <param name="password">User password</param>
        /// <param name="database">Database to connect to</param>
        /// <param name="encryptionProtocol">Encryption Protocol</param>
        /// <param name="identityClientId">Identity client ID for the UAMI</param>
        public TDSSQLTestClient(
            string server,
            int port,
            string authenticationType,
            string userID,
            string password,
            string database,
            SslProtocols encryptionProtocol = SslProtocols.Tls12,
            string authenticationLibrary = null,
            string identityClientId = null,
            bool trustServerCertificate = false)
        {
            ValidateInputParameters(server, userID, password, database, authenticationType);

            Client = null;
            Version = new TDSClientVersion(1, 0, 0, 0);
            Server = server;
            Port = port;
            UserID = userID;
            Password = password;
            Database = database;
            IdentityClientId = identityClientId;
            EncryptionProtocol = encryptionProtocol;
            ConnectionAttempt = 0;
            AuthenticationType = AuthTypeStringToEnum[authenticationType];
            TrustServerCertificate = trustServerCertificate;

            if (authenticationLibrary != null)
            {
                AuthLibStringToEnum.TryGetValue(authenticationLibrary, out AuthenticationLibrary);
            }

            LoggingUtilities.WriteLog($" Instantiating TDSSQLTestClient with the following parameters:");
            LoggingUtilities.WriteLog($"     Server: {server}.");
            LoggingUtilities.WriteLog($"     Port: {port}.");
            LoggingUtilities.WriteLog($"     UserID: {userID}.");
            LoggingUtilities.WriteLog($"     Database: {database}.");
            LoggingUtilities.WriteLog($"     Authentication type: {authenticationType}.");
            LoggingUtilities.WriteLog($"     TrustServerCertificate: {trustServerCertificate}.");
        }

        /// <summary>
        /// Helper method for validating input parameters for client.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="userID"></param>
        /// <param name="password"></param>
        /// <param name="database"></param>
        /// <param name="authenticationType"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private void ValidateInputParameters(string server, string userID, string password, string database, string authenticationType)
        {
            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(database) || string.IsNullOrEmpty(authenticationType))
            {
                throw new ArgumentNullException();
            }
            if (authenticationType.Contains("Microsoft Entra Password") || authenticationType.Contains("SQL Server Authentication"))
            {
                if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(password))
                {
                    throw new ArgumentNullException();
                }
            }
        }

        /// <summary>
        /// Connect to the server.
        /// </summary>
        public void Connect()
        {
            DateTime connectStartTime = DateTime.UtcNow;
            bool preLoginDone = false;
            var originalServerName = Server;
            var originalPort = Port;

            LoggingUtilities.WriteLog($"Connect initiated (attempt # {++ConnectionAttempt}).", writeToSummaryLog: true);

            try
            {
                do
                {
                    preLoginDone = false;
                    Reconnect = false;
                    var preLoginResponse = PerformPreLogin(ref preLoginDone);
                    PerformLogin(preLoginResponse);

                    if (Reconnect)
                    {
                        Disconnect();
                        LoggingUtilities.AddEmptyLine();
                        LoggingUtilities.WriteLog($"Routing to: {Server}:{Port}.");
                        LoggingUtilities.AddEmptyLine();
                    }
                }
                while (Reconnect);
            }
            catch (Exception ex)
            {
                if (!preLoginDone && DateTime.UtcNow >= connectStartTime.AddSeconds(5))
                {
                    LoggingUtilities.WriteLog($" SNI timeout detected, PreLogin phase was not complete after {(int)(DateTime.UtcNow - connectStartTime).TotalMilliseconds} milliseconds.",
                        writeToSummaryLog: true);
                }

                LoggingUtilities.AddEmptyLine();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Error");
                sb.AppendLine($"Exception:{ex.Message}");
                if (ex.InnerException != null)
                {
                    sb.AppendLine($"InnerException: {ex.InnerException.Message}");
                }
                LoggingUtilities.WriteLog(sb.ToString());
            }
            finally
            {
                Server = originalServerName;
                Port = originalPort;
            }
        }

        /// <summary>
        /// Execute the TDS Prelogin phase.
        /// </summary>
        /// <param name="preLoginDone"></param>
        /// <returns></returns>
        private TDSPreLoginPacketData PerformPreLogin(ref bool preLoginDone)
        {
            if (preLoginDone)
            {
                return null;
            }

            Reconnect = false;

            LoggingUtilities.WriteLog($"PreLogin phase starting", writeToSummaryLog: true);
            EstablishTCPConnection();

            DateTime connectStartTime = DateTime.UtcNow;

            SendPreLogin();
            TDSPreLoginPacketData preLoginResponse = (TDSPreLoginPacketData)ReceivePreLoginResponse();

            preLoginDone = true;
            LoggingUtilities.WriteLog($"PreLogin phase ended (took {(int)(DateTime.UtcNow - connectStartTime).TotalMilliseconds} ms)", writeToSummaryLog: true);

            return preLoginResponse;
        }

        /// <summary>
        /// Establishes TCP connection.
        /// </summary>
        private void EstablishTCPConnection()
        {
            MeasureDNSResolutionTime();
            Client = new TcpClient(Server, Port);
            ushort packetSize = 4096;
            TdsCommunicator = new TDSCommunicator(Client.GetStream(), packetSize, AuthenticationType);

            LoggingUtilities.WriteLog($"  TCP connection open between local {Client.Client.LocalEndPoint} and remote {Client.Client.RemoteEndPoint}",
                writeToVerboseLog: false,
                writeToSummaryLog: true);
            LoggingUtilities.WriteLog($"  TCP connection open");
            LoggingUtilities.WriteLog($"   Local endpoint is {Client.Client.LocalEndPoint}");
            LoggingUtilities.WriteLog($"   Remote endpoint is {Client.Client.RemoteEndPoint}");
        }

        /// <summary>
        /// Execute the login phase.
        /// </summary>
        /// <param name="preLoginResponse"></param>
        private void PerformLogin(TDSPreLoginPacketData preLoginResponse)
        {
            LoggingUtilities.AddEmptyLine();
            LoggingUtilities.WriteLog($"Starting Login phase.", writeToSummaryLog: true);
            DateTime connectStartTime = DateTime.UtcNow;
            SendLogin7();
            ReceiveLogin7Response();
            LoggingUtilities.WriteLog($"Login phase ended (took {(int)(DateTime.UtcNow - connectStartTime).TotalMilliseconds} ms)", writeToSummaryLog: true);
        }

        /// <summary>
        /// Sends PreLogin request message to the server.
        /// </summary>
        private void SendPreLogin()
        {
            LoggingUtilities.AddEmptyLine();
            LoggingUtilities.WriteLog($" Building PreLogin message.");

            var tdsMessageBody = new TDSPreLoginPacketData(Version);

            if (IsAADAuthRequired())
            {
                tdsMessageBody.AddOption(TDSPreLoginOptionTokenType.FedAuthRequired,
                                        TdsPreLoginFedAuthRequiredOption.FedAuthRequired);
            }

            tdsMessageBody.Terminate();

            TdsCommunicator.SendTDSMessage(tdsMessageBody);
            LoggingUtilities.WriteLog($" PreLogin message sent.");
        }

        /// <summary>
        /// Sends Login7 message to the server.
        /// </summary>
        private void SendLogin7()
        {
            LoggingUtilities.WriteLog($" Building Login7 message.");

            var tdsMessageBody = new TDSLogin7PacketData(Environment.MachineName, "TDSSQLTestClient", Server, Database);

            // If SQL Authentication is used, a part of the Login message are user id and password.
            //
            if (!IsAADAuthRequired())
            {
                LoggingUtilities.WriteLog($"  Adding SQL Authentication options");
                tdsMessageBody.AddLogin7SQLAuthenticationOptions(UserID, Password);
            }
            else
            {
                LoggingUtilities.WriteLog($"  Adding Entra Authentication options");
                tdsMessageBody.AddLogin7AADAuthenticationOptions(AuthenticationType);
            }

            TdsCommunicator.SendTDSMessage(tdsMessageBody);
            LoggingUtilities.WriteLog($" Login7 message sent.");
        }

        /// <summary>
        /// Send Fedauth message containing access token to the server
        /// </summary>
        /// <param name="accessToken"></param>
        private void SendFedAuthMessage(string accessToken)
        {
            LoggingUtilities.AddEmptyLine();
            LoggingUtilities.WriteLog($"  Sending JWT token to the server.");
            TDSFedAuthToken fedAuthToken = new TDSFedAuthToken(accessToken);
            TdsCommunicator.SendTDSMessage(fedAuthToken);
            LoggingUtilities.WriteLog($"  JWT token successfully sent.");
        }

        /// <summary>
        /// Receive PreLogin response from the server.
        /// </summary>
        private ITDSPacketData ReceivePreLoginResponse()
        {
            LoggingUtilities.WriteLog($" Waiting for PreLogin response.");

            ITDSPacketData preLoginResponse = TdsCommunicator.ReceiveTDSMessage();

            if (preLoginResponse is TDSPreLoginPacketData response)
            {
                if (response.Options.Exists(opt => opt.Type == TDSPreLoginOptionTokenType.Encryption) &&
                    response.Encryption == TDSEncryptionOption.EncryptReq)
                {
                    TdsCommunicator.EnableEncryption(Server, EncryptionProtocol, TrustServerCertificate);
                }
            }
            else
            {
                throw new InvalidOperationException();
            }

            LoggingUtilities.WriteLog($" PreLogin response processed.");

            return preLoginResponse;
        }

        /// <summary>
        /// Receive Login7 response from the server.
        /// </summary>
        private void ReceiveLogin7Response()
        {
            LoggingUtilities.AddEmptyLine();
            LoggingUtilities.WriteLog($" Waiting for Login7 response.");

            if (TdsCommunicator.ReceiveTDSMessage() is TDSTokenStreamPacketData response)
            {
                foreach (var token in response.Tokens)
                {
                    if (token is TDSEnvChangeToken)
                    {
                        ProcessEnvChangeToken(token as TDSEnvChangeToken);
                    }
                    else
                    {
                        if (token is TDSFedAuthInfoToken)
                        {
                            Tuple<string, string> fedAuthInfoMessage = ProcessFedAuthInfoToken((TDSFedAuthInfoToken)token);
                            string authority = fedAuthInfoMessage.Item1;
                            string resource = fedAuthInfoMessage.Item2;

                            AuthenticationProvider authenticationProvider = new AuthenticationProvider(AuthenticationLibrary, AuthenticationType, UserID, Password, authority, resource, IdentityClientId);
                            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
                            string accessToken = null;

                            var task = Task.Run(async () => { return await authenticationProvider.GetJWTAccessToken(); }, cts.Token);
                            var completedTask = Task.WhenAny(task, Task.Delay(Timeout.Infinite, cts.Token)).GetAwaiter().GetResult();

                            if (completedTask == task)
                            {
                                accessToken = task.GetAwaiter().GetResult(); // Task completed within timeout
                                SendFedAuthMessage(accessToken);

                                LoggingUtilities.AddEmptyLine();
                                LoggingUtilities.WriteLog($"  Waiting for the response from the server.");

                                if (TdsCommunicator.ReceiveTDSMessage() is TDSTokenStreamPacketData accessTokenResponse)
                                {
                                    foreach (var responseToken in accessTokenResponse.Tokens)
                                    {
                                        if (responseToken is TDSEnvChangeToken)
                                        {
                                            ProcessEnvChangeToken(responseToken as TDSEnvChangeToken);
                                        }
                                        else
                                        {
                                            responseToken.ProcessToken();
                                        }
                                    }
                                }
                                LoggingUtilities.WriteLog($" Response processed.");
                            }
                            else
                            {
                                cts.Cancel();
                                throw new Exception("Operation timed out afer 2 minutes.");
                            }
                        }
                        else
                        {
                            token.ProcessToken();
                        }
                    }
                }
            }
            else
            {
                throw new InvalidOperationException();
            }

            LoggingUtilities.WriteLog($" Login7 response processed.");
        }

        /// <summary>
        /// Helper method which processes the env change token recevied from the server.
        /// </summary>
        /// <param name="envChangeToken"></param>
        private void ProcessEnvChangeToken(TDSEnvChangeToken envChangeToken)
        {
            LoggingUtilities.WriteLog($"  Processing EnvChange {envChangeToken.Type} token.");

            if (envChangeToken.Type == Tokens.EnvChange.TDSEnvChangeType.Routing)
            {
                LoggingUtilities.WriteLog($"     Client is being routed.");
                Server = envChangeToken.Values["AlternateServer"];
                Port = int.Parse(envChangeToken.Values["ProtocolProperty"]);
                Reconnect = true;
                TdsCommunicator.CommunicatorState = TDSCommunicatorState.Initial;
                LoggingUtilities.WriteLog($"     Redirect to {Server}:{Port}", writeToSummaryLog: true, writeToVerboseLog: false);
            }
        }

        /// <summary>
        /// Helper method which processes the fed auth info token recevied from the server.
        /// </summary>
        /// <param name="fedAuthInfoToken"></param>
        /// <returns></returns>
        private Tuple<string, string> ProcessFedAuthInfoToken(TDSFedAuthInfoToken fedAuthInfoToken)
        {
            LoggingUtilities.WriteLog($"   Client received FedAuthInfo token");

            string STSUrl = null;
            string SPN = null;

            foreach (KeyValuePair<int, TDSFedAuthInfoOption> option in fedAuthInfoToken.Options)
            {
                ProcessFedAuthInfoOption(option, ref STSUrl, ref SPN);
            }

            return new Tuple<string, string>(STSUrl, SPN);
        }

        /// <summary>
        /// Helper method which processes the fed auth info option token recevied from the server.
        /// </summary>
        /// <param name="option"></param>
        /// <param name="STSUrl"></param>
        /// <param name="SPN"></param>
        private void ProcessFedAuthInfoOption(KeyValuePair<int, TDSFedAuthInfoOption> option, ref string STSUrl, ref string SPN)
        {
            if (option.Value.FedAuthInfoId == TDSFedAuthInfoId.STSURL)
            {
                TDSFedAuthInfoOptionSTSURL optionSTSURL = option.Value as TDSFedAuthInfoOptionSTSURL;
                var output = optionSTSURL.StsUrl.Where(b => b != 0).ToArray();
                STSUrl = Encoding.UTF8.GetString(output);
                LoggingUtilities.WriteLog($"     STSURL: {STSUrl}");
            }
            else if (option.Value.FedAuthInfoId == TDSFedAuthInfoId.SPN)
            {
                TDSFedAuthInfoOptionSPN optionSPN = option.Value as TDSFedAuthInfoOptionSPN;
                var output = optionSPN.ServicePrincipalName.Where(b => b != 0).ToArray();
                SPN = Encoding.UTF8.GetString(output);
                LoggingUtilities.WriteLog($"     Service Principal Name: {SPN}");
            }

            LoggingUtilities.AddEmptyLine();
        }

        /// <summary>
        /// Disconnect from the server.
        /// </summary>
        public void Disconnect()
        {
            if (Client != null)
            {
                LoggingUtilities.AddEmptyLine();
                LoggingUtilities.WriteLog($" Disconnect initiated.", writeToSummaryLog: true);
                Client.Close();
                Client = null;
                LoggingUtilities.WriteLog($" Disconnect done.", writeToSummaryLog: true);
            }
        }

        /// <summary>
        /// Check if the requested authentication is AAD authentication.
        /// </summary>
        /// <returns></returns>
        private bool IsAADAuthRequired()
        {
            var aadAuthTypes = new TDSAuthenticationType[] {
                TDSAuthenticationType.ADPassword,
                TDSAuthenticationType.ADIntegrated,
                TDSAuthenticationType.ADInteractive,
                TDSAuthenticationType.ADManagedIdentity };

            return aadAuthTypes.Contains(AuthenticationType);
        }

        /// <summary>
        /// Measure time needed for DNS resolution.
        /// </summary>
        private void MeasureDNSResolutionTime()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var addresses = Dns.GetHostAddresses(this.Server);
                stopwatch.Stop();
                var addressListString = string.Join(",", addresses.AsEnumerable());
                LoggingUtilities.WriteLog($" DNS resolution took {stopwatch.ElapsedMilliseconds} ms, ({addressListString})", writeToSummaryLog: true);
            }
            catch (SocketException socketException)
            {
                LoggingUtilities.WriteLog($" DNS resolution failed with \"{socketException.Message}\", error {socketException.NativeErrorCode} for address {this.Server}", writeToSummaryLog: true);
            }
            catch (Exception ex)
            {
                LoggingUtilities.WriteLog($" DNS resolution failed with \"{ex.Message}\", for address {this.Server}", writeToSummaryLog: true);
            }
        }
    }
}
