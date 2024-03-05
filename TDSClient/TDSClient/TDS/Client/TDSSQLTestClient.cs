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

    using TDSClient.TDS.Comms;
    using TDSClient.TDS.Header;
    using TDSClient.TDS.Login7;
    using TDSClient.TDS.PreLogin;
    using TDSClient.TDS.Utilities;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.FedAuthInfo;
    using TDSClient.TDS.FedAuthMessage;
    using TDSClient.MSALHelper;
    using TDSClient.ADALHelper;
    using TDSClient.TDS.Tokens;

    public enum TDSAuthenticationType
        {
            SQLServerAuthentication,
            ADPassword,
            ADIntegrated,
            ADInteractive,
            ADManagedIdentity
        }

    /// <summary>
    /// SQL Test Client used to run diagnostics on SQL Server using TDS protocol.
    /// </summary>
    public class TDSSQLTestClient
    {
        private static readonly Dictionary<string, TDSAuthenticationType> authTypeStringToEnum = new Dictionary<string, TDSAuthenticationType>
        {
            { "SQL Server Authentication", TDSAuthenticationType.SQLServerAuthentication },
            { "Active Directory Password", TDSAuthenticationType.ADPassword },
            { "Active Directory Integrated", TDSAuthenticationType.ADIntegrated },
            { "Active Directory Interactive", TDSAuthenticationType.ADInteractive },
            { "Active Directory Managed Identity", TDSAuthenticationType.ADManagedIdentity },
            { "Active Directory MSI", TDSAuthenticationType.ADManagedIdentity }
        };

        private bool Reconnect;
        private int ConnectionAttempt;
        private readonly TDSAuthenticationType AuthenticationType;
        private readonly string AuthenticationLibrary;
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
            string authenticationLibrary,
            string userID,
            string password,
            string database,
            SslProtocols encryptionProtocol = SslProtocols.Tls12,
            string identityClientId = null)
        {
            ValidateInputParameters(server, userID, password, database, authenticationType);

            Client = null;
            Version = new TDSClientVersion(1, 0, 0, 0);
            Server = server;
            Port = port;
            UserID = userID;
            Password = password;
            Database = database;

            if (identityClientId != null)
            {
                IdentityClientId = identityClientId;
            }

            EncryptionProtocol = encryptionProtocol;
            ConnectionAttempt = 0;

            AuthenticationType = authTypeStringToEnum[authenticationType];
            AuthenticationLibrary = authenticationLibrary;

            LoggingUtilities.WriteLog($" Instantiating TDSSQLTestClient with the following parameters:");

            LoggingUtilities.WriteLog($"     Server: {server}.");
            LoggingUtilities.WriteLog($"     Port: {port}.");
            LoggingUtilities.WriteLog($"     UserID: {userID}.");
            LoggingUtilities.WriteLog($"     Database: {database}.");
            LoggingUtilities.WriteLog($"     Authentication type: {authenticationType}.");
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
            if (authenticationType.Contains("Active Directory Password") || authenticationType.Contains("SQL Server Authentication"))
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
        public async Task<bool> Connect()
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
                    var preLoginResponse = PerformPreLogin(ref preLoginDone);
                    await PerformLogin(preLoginResponse);

                    if (Reconnect)
                    {
                        Disconnect();
                        LoggingUtilities.AddEmptyLine();
                        LoggingUtilities.WriteLog($" Routing to: {Server}:{Port}.");
                    }

                    return true;
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
                LoggingUtilities.WriteLog($"Exception:");
                LoggingUtilities.WriteLog($"{ex.Message}");

                if (ex.InnerException != null)
                {
                    LoggingUtilities.WriteLog($"InnerException: {ex.InnerException.Message}");
                }

                return false;

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
            Reconnect = false;

            EstablishTCPConnection();

            DateTime connectStartTime = DateTime.UtcNow;

            SendPreLogin();
            TDSPreLoginPacketData preLoginResponse = (TDSPreLoginPacketData)ReceivePreLoginResponse();

            preLoginDone = true;
            LoggingUtilities.AddEmptyLine();
            LoggingUtilities.WriteLog($" PreLogin phase took {(int)(DateTime.UtcNow - connectStartTime).TotalMilliseconds} milliseconds.") ;

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

            LoggingUtilities.WriteLog($"  TCP connection open between local {Client.Client.LocalEndPoint} and remote {Client.Client.RemoteEndPoint}", writeToVerboseLog: false, writeToSummaryLog: true);
            LoggingUtilities.WriteLog($"  TCP connection open");
            LoggingUtilities.WriteLog($"   Local endpoint is {Client.Client.LocalEndPoint}");
            LoggingUtilities.WriteLog($"   Remote endpoint is {Client.Client.RemoteEndPoint}");
        }

        /// <summary>
        /// Execute the login phase
        /// </summary>
        /// <param name="preLoginResponse"></param>
        private async Task PerformLogin(TDSPreLoginPacketData preLoginResponse)
        {
            DateTime connectStartTime = DateTime.UtcNow;

            SendLogin7();

            // 1. If AAD authentication is used, client should receive a fed auth message from the server.
            //    After that, the client tries to acquire an access token from AAD using ADAL/MSAL.
            //    If it acquires it, it sends it to the server, and receives a Login response.
            // 2. Else if SQL authentication is used, the client should receive a Login response.
            //
            if (preLoginResponse.Options.Exists(opt => opt.Type == TDSPreLoginOptionTokenType.FedAuthRequired) &&
                preLoginResponse.FedAuthRequired == TdsPreLoginFedAuthRequiredOption.FedAuthRequired &&
                IsAADAuthRequired())
            {
                Tuple<string, string> fedAuthInfoMessage = ReceiveFedAuthInfoMessage();
                string authority = fedAuthInfoMessage.Item1;
                string resource = fedAuthInfoMessage.Item2;

                string accessToken = await GetJWTAccessToken(authority, resource);

                SendFedAuthMessage(accessToken);
            }

            ReceiveLogin7Response();

            LoggingUtilities.WriteLog($" Login phase took {(int)(DateTime.UtcNow - connectStartTime).TotalMilliseconds} milliseconds.");
        }

        /// <summary>
        /// Acquires JWT Access token.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <param name="clientID"></param>
        /// <returns></returns>
        private async Task<string> GetJWTAccessToken(string authority, string resource)
        {
            string accessToken = null;

            switch(AuthenticationType)
            {
                case TDSAuthenticationType.ADIntegrated:
                    accessToken = await GetAccessTokenForIntegratedAuth(authority, resource);
                    break;
                case TDSAuthenticationType.ADInteractive:
                    accessToken = await GetAccessTokenForInteractiveAuth(authority);
                    break;
                case TDSAuthenticationType.ADPassword:
                    accessToken = await GetAccessTokenForUsernamePassword(authority, resource);
                    break;
                case TDSAuthenticationType.ADManagedIdentity:
                    accessToken = await GetAccessTokenForMSIAuth(authority);
                    break;
            }

            return accessToken;
        }

        /// <summary>
        /// Acquires access token for AAD integrated authentication.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <param name="clientID"></param>
        /// <returns></returns>
        private async Task<string> GetAccessTokenForIntegratedAuth(string authority, string resource)
        {
            return AuthenticationLibrary.Contains("MSAL") ?
                await MSALHelper.GetSQLAccessTokenFromMSALUsingIntegratedAuth(authority, resource, UserID) :
                await ADALHelper.GetSQLAccessTokenFromADALUsingIntegratedAuth(authority, resource);
        }

        /// <summary>
        /// Acquires access token for AAD username password authentication.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <param name="clientID"></param>
        /// <returns></returns>
        private async Task<string> GetAccessTokenForUsernamePassword(string authority, string resource)
        {
             return AuthenticationLibrary.Contains("MSAL") ?
                await MSALHelper.GetSQLAccessTokenFromMSALUsingUsernamePassword(authority, resource, UserID, Password) :
                throw new Exception("Username password authentication is not supported by ADAL.");
        }

        /// <summary>
        /// Acquires access token for AAD integrated authentication.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <param name="clientID"></param>
        /// <returns></returns>
        private async Task<string> GetAccessTokenForInteractiveAuth(string authority)
        {
            return await MSALHelper.GetSQLAccessTokenFromMSALInteractively(authority);
        }

        /// <summary>
        /// Acquires access token for AAD integrated authentication.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <param name="clientID"></param>
        /// <returns></returns>
        private async Task<string> GetAccessTokenForMSIAuth(string authority)
        {
            return IdentityClientId != null ?
                await MSALHelper.GetSQLAccessTokenFromMSALUsingUserAssignedManagedIdentity(authority, IdentityClientId) :
                await MSALHelper.GetSQLAccessTokenFromMSALUsingSystemAssignedManagedIdentity(authority);
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
            LoggingUtilities.AddEmptyLine();
            LoggingUtilities.WriteLog($" Building Login7 message.");

            var tdsMessageBody = new TDSLogin7PacketData();

            LoggingUtilities.WriteLog($" Adding option HostName with value [{Environment.MachineName}]");
            tdsMessageBody.HostName = Environment.MachineName;

            LoggingUtilities.WriteLog($"  Adding option ApplicationName with value [TDSSQLTestClient]");
            tdsMessageBody.ApplicationName = "TDSSQLTestClient";

            LoggingUtilities.WriteLog($"  Adding option ServerName with value [{Server}]");
            tdsMessageBody.ServerName = Server;

            LoggingUtilities.WriteLog($"  Adding option Database with value [{Database}]");
            tdsMessageBody.Database = Database;

            tdsMessageBody.ClientTimeZone = 480;

            // If SQL authentication is used, a part of the Login message is a user id and a password.
            //
            if (!IsAADAuthRequired())
            {
                LoggingUtilities.WriteLog($"  Adding SQL Authentication options");
                AddLogin7SQLAuthenticationOptions(tdsMessageBody);
            }
            else
            {
                LoggingUtilities.WriteLog($"  Adding AAD Authentication options");
                AddLogin7AADAuthenticationOptions(tdsMessageBody);
            }

            LoggingUtilities.WriteLog($"  Adding common login options");

            TdsCommunicator.SendTDSMessage(tdsMessageBody);

            LoggingUtilities.WriteLog($" Login7 message sent.");
        }

        /// <summary>
        /// Adds options for SQL Authentication to TDS Login message.
        /// </summary>
        /// <param name="tdsMessageBody"></param>

        private void AddLogin7SQLAuthenticationOptions(TDSLogin7PacketData tdsMessageBody)
        {
            LoggingUtilities.WriteLog($"  Adding option UserID with value [{UserID}]");
            tdsMessageBody.UserID = UserID;

            LoggingUtilities.WriteLog($"  Adding option Password");
            tdsMessageBody.Password = Password;

            tdsMessageBody.OptionFlags3.Extension = TDSLogin7OptionFlags3Extension.DoesntExist;
        }

        /// <summary>
        /// Adds options for AAD Authentication to TDS Login message.
        /// </summary>
        /// <param name="tdsMessageBody"></param>
        private void AddLogin7AADAuthenticationOptions(TDSLogin7PacketData tdsMessageBody)
        {
            TDSFedAuthADALWorkflow adalWorkflow = AuthenticationType.Equals(TDSAuthenticationType.ADIntegrated) ?
                TDSFedAuthADALWorkflow.Integrated : TDSFedAuthADALWorkflow.UserPassword;

            tdsMessageBody.OptionFlags3.Extension = TDSLogin7OptionFlags3Extension.Exists;

            TDSLogin7FedAuthOptionToken featureOption = CreateLogin7FederatedAuthenticationFeatureExt(TDSFedAuthLibraryType.ADAL, adalWorkflow);

            tdsMessageBody.FeatureExt ??= new TDSLogin7FeatureOptionsToken();
            tdsMessageBody.FeatureExt.Add(featureOption);
        }

        /// <summary>
        /// Receives and handles a federated authentication info response from server.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        private Tuple<string, string> ReceiveFedAuthInfoMessage()
        {
            LoggingUtilities.AddEmptyLine();
            LoggingUtilities.WriteLog($" Waiting for FedAuthInfoMessage response.");

            if (TdsCommunicator.ReceiveTDSMessage() is TDSTokenStreamPacketData response)
            {
                foreach (var token in response.Tokens)
                {
                    if (token is TDSEnvChangeToken)
                    {
                        ProcessEnvChangeToken(token as TDSEnvChangeToken);
                    }
                    else if (token is TDSFedAuthInfoToken)
                    {
                        return ProcessFedAuthInfoToken(token as TDSFedAuthInfoToken);
                    }
                    else if (token is TDSErrorToken)
                    {
                        token.ProcessToken();
                    }
                }

                throw new Exception("Server couldn't return a proper Fed Auth Info message.");

            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Send Fedauth message containing access token to the server
        /// </summary>
        /// <param name="accessToken"></param>
        private void SendFedAuthMessage(string accessToken)
        {
            LoggingUtilities.WriteLog($"  Sending JWT token to the server.");
            TDSFedAuthToken fedAuthToken = new TDSFedAuthToken(accessToken);
            TdsCommunicator.SendTDSMessage(fedAuthToken);
            LoggingUtilities.WriteLog($"  JWT token successfully sent.");
        }

        /// <summary>
        /// Creates Fedauth feature extension for the login7 packet.
        /// </summary>
        private TDSLogin7FedAuthOptionToken CreateLogin7FederatedAuthenticationFeatureExt(TDSFedAuthLibraryType libraryType, TDSFedAuthADALWorkflow workflow = TDSFedAuthADALWorkflow.EMPTY)
        {
            // Create feature option
            TDSLogin7FedAuthOptionToken featureOption =
                new TDSLogin7FedAuthOptionToken(TdsPreLoginFedAuthRequiredOption.FedAuthRequired,
                                                libraryType,
                                                null,
                                                null,
                                                null,
                                                false,
                                                libraryType == TDSFedAuthLibraryType.ADAL,
                                                workflow);

            return featureOption;
        }

        /// <summary>
        /// Receive PreLogin response from the server.
        /// </summary>
        private ITDSPacketData ReceivePreLoginResponse()
        {
            LoggingUtilities.AddEmptyLine();
            LoggingUtilities.WriteLog($" Waiting for PreLogin response.");

            ITDSPacketData preLoginResponse = TdsCommunicator.ReceiveTDSMessage();
            if (preLoginResponse is TDSPreLoginPacketData response)
            {
                if (response.Options.Exists(opt => opt.Type == TDSPreLoginOptionTokenType.Encryption) &&
                    response.Encryption == TDSEncryptionOption.EncryptReq)
                {
                    LoggingUtilities.WriteLog($"  Server requires encryption, enabling encryption.");
                    TdsCommunicator.EnableEncryption(Server, EncryptionProtocol);
                    LoggingUtilities.WriteLog($"  Encryption enabled.");
                }
            }
            else
            {
                throw new InvalidOperationException();
            }

            LoggingUtilities.WriteLog($" PreLogin response received.");

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
                        token.ProcessToken();
                    }
                }
            }
            else
            {
                throw new InvalidOperationException();
            }

            LoggingUtilities.WriteLog($" Login7 response received.");
        }

        /// <summary>
        /// Helper method which processes the env change token recevied from the server.
        /// </summary>
        /// <param name="envChangeToken"></param>
        private void ProcessEnvChangeToken(TDSEnvChangeToken envChangeToken)
        {
            if (envChangeToken.Type == Tokens.EnvChange.TDSEnvChangeType.Routing)
            {
                LoggingUtilities.WriteLog($" Client received EnvChange routing token, client is being routed.");
                Server = envChangeToken.Values["AlternateServer"];
                Port = int.Parse(envChangeToken.Values["ProtocolProperty"]);
                Reconnect = true;
                LoggingUtilities.WriteLog($" Redirect to {Server}:{Port}", writeToSummaryLog: true, writeToVerboseLog: false);
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
        }

        /// <summary>
        /// Disconnect from the server.
        /// </summary>
        public void Disconnect()
        {
            if (Client != null)
            {
                LoggingUtilities.AddEmptyLine();
                LoggingUtilities.WriteLog($" Disconnect initiated.");
                Client.Close();
                Client = null;
                LoggingUtilities.WriteLog($" Disconnect done.");
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
