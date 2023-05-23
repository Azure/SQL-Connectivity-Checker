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

    using System.Data.SqlClient;

    using TDSClient.TDS.Comms;
    using TDSClient.TDS.Header;
    using TDSClient.TDS.Login7;
    using TDSClient.TDS.PreLogin;
    using TDSClient.TDS.Tokens;
    using TDSClient.TDS.Utilities;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.LoginAck;
    using TDSClient.TDS.FedAuthInfo;

    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// SQL Test Client used to run diagnostics on SQL Server using TDS protocol.
    /// </summary>
    public class TDSSQLTestClient
    {
        /// <summary>
        /// Field describing whether reconnection is required
        /// </summary>
        private bool reconnect;

        private int connectionAttempt;

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
        public TDSSQLTestClient(string server, int port, string authenticationType, string userID, string password, string database, SslProtocols encryptionProtocol = SslProtocols.Tls12)
        {
            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(database) || string.IsNullOrEmpty(authenticationType))
            {
                throw new ArgumentNullException();
            }

            this.Client = null;
            this.Version = new TDSClientVersion(1, 0, 0, 0);
            this.Server = server;
            this.Port = port;
            this.UserID = userID;
            this.Password = password;
            this.Database = database;
            this.EncryptionProtocol = encryptionProtocol;
            this.connectionAttempt = 0;
            this.AuthenticationType = authenticationType;

            LoggingUtilities.WriteLog($" Instantiating TDSSQLTestClient with the following parameters:");

            LoggingUtilities.WriteLog($"     Server: {server}.");
            LoggingUtilities.WriteLog($"     Port: {port}.");
            LoggingUtilities.WriteLog($"     UserID: {userID}.");
            LoggingUtilities.WriteLog($"     Database: {database}.");
            LoggingUtilities.WriteLog($"     Authentication type: {authenticationType}.");
        }

        /// <summary>
        /// Gets or sets the authentication type.
        /// </summary>
        public string AuthenticationType { get; set; }

        /// <summary>
        /// Gets or sets the Server.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the Port Number.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the User ID.
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// Gets or sets the Password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the Database.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Gets or sets the TDS Communicator.
        /// </summary>
        public TDSCommunicator TdsCommunicator { get; set; }

        /// <summary>
        /// Gets or sets the TCP Client.
        /// </summary>
        public TcpClient Client { get; set; }

        /// <summary>
        /// Gets or sets the TDS Client Version.
        /// </summary>
        public TDSClientVersion Version { get; set; }

        /// <summary>
        /// Gets or sets the Encryption Protocol.
        /// </summary>
        public SslProtocols EncryptionProtocol { get; set; }

        /// <summary>
        /// Sends PreLogin message to the server.
        /// </summary>
        public void SendPreLogin()
        {
            LoggingUtilities.AddEmptyLine();
            LoggingUtilities.WriteLog($" Building PreLogin message.");
            var tdsMessageBody = new TDSPreLoginPacketData(this.Version);

            tdsMessageBody.AddOption(TDSPreLoginOptionTokenType.Encryption, TDSEncryptionOption.EncryptOff);
            tdsMessageBody.AddOption(TDSPreLoginOptionTokenType.TraceID, new TDSClientTraceID(Guid.NewGuid().ToByteArray(), Guid.NewGuid().ToByteArray(), 0));

            // add fed auth required option if we are using AAD authentication
            if(this.AuthenticationType.Equals("Azure Active Directory Password"))
            {
                tdsMessageBody.AddOption(TDSPreLoginOptionTokenType.FedAuthRequired, TdsPreLoginFedAuthRequiredOption.FedAuthRequired);
            }

            tdsMessageBody.Terminate();

            this.TdsCommunicator.SendTDSMessage(tdsMessageBody);
            LoggingUtilities.WriteLog($" PreLogin message sent.");
        }

        /// <summary>
        /// Sends Login7 message to the server
        /// </summary>
        public void SendLogin7(bool isSqlAuth)
        {
            LoggingUtilities.AddEmptyLine();
            LoggingUtilities.WriteLog($" Building Login7 message.");

            var tdsMessageBody = new TDSLogin7PacketData();

            LoggingUtilities.WriteLog($"    Adding option HostName with value {Environment.MachineName}");
            tdsMessageBody.HostName = Environment.MachineName;

            LoggingUtilities.WriteLog($"    Adding option ApplicationName with value TDSSQLTestClient");
            tdsMessageBody.ApplicationName = "TDSSQLTestClient";

            tdsMessageBody.ClientTimeZone = 480;

            if (isSqlAuth)
            {
                LoggingUtilities.WriteLog($"    Adding option UserID with value {this.UserID}");
                tdsMessageBody.UserID = this.UserID;
                
                LoggingUtilities.WriteLog($"    Adding option Password");
                tdsMessageBody.Password = this.Password; 
            }

            LoggingUtilities.WriteLog($"    Adding option ServerName with value {this.Server}");
            tdsMessageBody.ServerName = this.Server;

            LoggingUtilities.WriteLog($"    Adding option Database with value {this.Database}");
            tdsMessageBody.Database = this.Database;

            tdsMessageBody.OptionFlags1.Char = TDSLogin7OptionFlags1Char.CharsetASCII;
            tdsMessageBody.OptionFlags1.Database = TDSLogin7OptionFlags1Database.InitDBFatal;
            tdsMessageBody.OptionFlags1.DumpLoad = TDSLogin7OptionFlags1DumpLoad.DumploadOn;
            tdsMessageBody.OptionFlags1.Float = TDSLogin7OptionFlags1Float.FloatIEEE754;
            tdsMessageBody.OptionFlags1.SetLang = TDSLogin7OptionFlags1SetLang.SetLangOn;
            tdsMessageBody.OptionFlags1.ByteOrder = TDSLogin7OptionFlags1ByteOrder.OrderX86;
            tdsMessageBody.OptionFlags1.UseDB = TDSLogin7OptionFlags1UseDB.UseDBOff;

            if (this.AuthenticationType.Contains("Integrated"))
            {
                // Enable integrated authentication
                tdsMessageBody.OptionFlags2.IntSecurity = TDSLogin7OptionFlags2IntSecurity.IntegratedSecurityOn;

                // Generate client context
				// (Context as GenericTDSClientContext).NTUserAuthenticationContext = SSPIContext.CreateClient();

				// // Create a request mesage
				// SSPIResponse request = (Context as GenericTDSClientContext).NTUserAuthenticationContext.StartClientAuthentication(Context.ServerHost, Context.ServerPort);

				// // Put SSPI block into the login packet
				// loginToken.SSPI = request.Payload;
            }
            else
            {
                // Turn off integrated authentication
                tdsMessageBody.OptionFlags2.IntSecurity = TDSLogin7OptionFlags2IntSecurity.IntegratedSecurityOff;
            }

            tdsMessageBody.OptionFlags2.Language = TDSLogin7OptionFlags2Language.InitLangFatal;
            tdsMessageBody.OptionFlags2.ODBC = TDSLogin7OptionFlags2ODBC.OdbcOn;
            tdsMessageBody.OptionFlags2.UserType = TDSLogin7OptionFlags2UserType.UserNormal;

            tdsMessageBody.OptionFlags3.ChangePassword = TDSLogin7OptionFlags3ChangePassword.NoChangeRequest;
            tdsMessageBody.OptionFlags3.UserInstanceProcess = TDSLogin7OptionFlags3UserInstanceProcess.DontRequestSeparateProcess;
            tdsMessageBody.OptionFlags3.UnknownCollationHandling = TDSLogin7OptionFlags3UnknownCollationHandling.On;

            if (isSqlAuth)
            {
                tdsMessageBody.OptionFlags3.Extension = TDSLogin7OptionFlags3Extension.DoesntExist;
            }
            else
            {
                tdsMessageBody.OptionFlags3.Extension = TDSLogin7OptionFlags3Extension.Exists;

                TDSLogin7FedAuthOptionToken featureOption = CreateLogin7FederatedAuthenticationFeatureExt(TDSFedAuthLibraryType.ADAL, TDSFedAuthADALWorkflow.UserPassword);

                // Check if we have a feature extension
			    if (tdsMessageBody.FeatureExt == null)
			    {
				    // Create feature extension before using it
				    tdsMessageBody.FeatureExt = new TDSLogin7FeatureOptionsToken();
			    }
                tdsMessageBody.FeatureExt.Add(featureOption);
            }

            tdsMessageBody.TypeFlags.OLEDB = TDSLogin7TypeFlagsOLEDB.On;
            tdsMessageBody.TypeFlags.SQLType = TDSLogin7TypeFlagsSQLType.DFLT;
            tdsMessageBody.TypeFlags.ReadOnlyIntent = TDSLogin7TypeFlagsReadOnlyIntent.On;

            this.TdsCommunicator.SendTDSMessage(tdsMessageBody);

            LoggingUtilities.WriteLog($" Login7 message sent.");
        }

        /// <summary>
		/// Creates Fedauth feature extension for the login7 packet.
		/// </summary>
		protected virtual TDSLogin7FedAuthOptionToken CreateLogin7FederatedAuthenticationFeatureExt(TDSFedAuthLibraryType libraryType, TDSFedAuthADALWorkflow workflow = TDSFedAuthADALWorkflow.EMPTY)
		{
			// Create feature option
			TDSLogin7FedAuthOptionToken featureOption =
				new TDSLogin7FedAuthOptionToken(TdsPreLoginFedAuthRequiredOption.FedAuthRequired,
												libraryType,
												null,
												null,
												null /*channelBindingToken*/,
												false /*fIncludeSignature*/,
												libraryType == TDSFedAuthLibraryType.ADAL ? true : false /*fRequestingFurtherInfo*/,
												workflow);

			return featureOption;
		}

        /// <summary>
        /// Receive PreLogin response from the server.
        /// </summary>
        public ITDSPacketData ReceivePreLoginResponse()
        {
            LoggingUtilities.AddEmptyLine();
            LoggingUtilities.WriteLog($" Waiting for PreLogin response.");

            ITDSPacketData preLoginResponse = this.TdsCommunicator.ReceiveTDSMessage();
            if (preLoginResponse is TDSPreLoginPacketData response)
            {
                if (response.Options.Exists(opt => opt.Type == TDSPreLoginOptionTokenType.Encryption) && response.Encryption == TDSEncryptionOption.EncryptReq)
                {
                    LoggingUtilities.WriteLog($"  Server requires encryption, enabling encryption.");
                    this.TdsCommunicator.EnableEncryption(this.Server, this.EncryptionProtocol);
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
        public Tuple<string, string> ReceiveLogin7Response()
        {
            LoggingUtilities.AddEmptyLine();
            LoggingUtilities.WriteLog($" Waiting for Login7 response.");

            if (this.TdsCommunicator.ReceiveTDSMessage() is TDSTokenStreamPacketData response)
            {
                foreach (var token in response.Tokens)
                {
                    if (token is TDSEnvChangeToken)
                    {
                        var envChangeToken = token as TDSEnvChangeToken;
                        if (envChangeToken.Type == Tokens.EnvChange.TDSEnvChangeType.Routing)
                        {
                            LoggingUtilities.WriteLog($" Client received EnvChange routing token, client is being routed.");
                            this.Server = envChangeToken.Values["AlternateServer"];
                            this.Port = int.Parse(envChangeToken.Values["ProtocolProperty"]);
                            this.reconnect = true;
                            LoggingUtilities.WriteLog($" Redirect to {this.Server}:{this.Port}", writeToSummaryLog: true, writeToVerboseLog: false);
                        }
                        return null;

                    }
                    else if (token is TDSFedAuthInfoToken)
				    {
						LoggingUtilities.WriteLog($"   Client received FedAuthInfo token");
					    TDSFedAuthInfoToken fedAuthInfoToken = token as TDSFedAuthInfoToken;
                        
                        string STSUrl = null;
                        string SPN = null;
                        
                        foreach(KeyValuePair<int, TDSFedAuthInfoOption> option in fedAuthInfoToken.Options)
                        {
                            if (option.Value.FedAuthInfoId == TDSFedAuthInfoId.STSURL)
                            {
                                TDSFedAuthInfoOptionSTSURL optionSTSURL = option.Value as TDSFedAuthInfoOptionSTSURL;
                                var output = optionSTSURL.m_stsUrl.Where(b => b != 0).ToArray();

                                STSUrl = (Encoding.UTF8.GetString(output));
                                LoggingUtilities.WriteLog("    Token endpoint URL for acquiring Federated Authentication Token: " + STSUrl);
                            }
                            else if (option.Value.FedAuthInfoId == TDSFedAuthInfoId.SPN)
                            {
                                TDSFedAuthInfoOptionSPN optionSPN = option.Value as TDSFedAuthInfoOptionSPN;
                                var output = optionSPN.m_spn.Where(b => b != 0).ToArray();

                                SPN = (Encoding.UTF8.GetString(output));
                                LoggingUtilities.WriteLog("    Service Principal Name to use for acquiring Federated Authentication Token: " + SPN);
                            }
                        }

                        Tuple<string, string> t = new Tuple<string, string>(STSUrl, SPN);
                        return t;
                    }
                    else if (token is TDSLoginAckToken)
                    {
                        // Cast to login acknowledgement
					    TDSLoginAckToken loginAck = token as TDSLoginAckToken;

					    // Populate run time context
					    LoggingUtilities.WriteLog(loginAck.ServerName);
					    LoggingUtilities.WriteLog(loginAck.ServerVersion.ToString());
					    LoggingUtilities.WriteLog(loginAck.TDSVersion.ToString());

						LoggingUtilities.WriteLog("Logged in.");
                        return null;

                    }
                    else if (token is TDSErrorToken)
                    {
                        var errorToken = token as TDSErrorToken;
                        LoggingUtilities.WriteLog($" Client received Error token, Number: {errorToken.Number}, State: {errorToken.State}", writeToSummaryLog: true); ;
                        LoggingUtilities.WriteLog($"  MsgText: {errorToken.MsgText}");
                        LoggingUtilities.WriteLog($"  Class: {errorToken.Class}");
                        LoggingUtilities.WriteLog($"  ServerName: {errorToken.ServerName}");
                        LoggingUtilities.WriteLog($"  ProcName: {errorToken.ProcName}");
                        LoggingUtilities.WriteLog($"  LineNumber: {errorToken.LineNumber}");
                        LoggingUtilities.WriteLog($"  State: {errorToken.State}");

                        if (errorToken.Number == 18456)
                        {
                            throw new Exception("Login failure.");
                        }

                        return null;

                    }
                    else if (token is TDSInfoToken)
                    {
                        var infoToken = token as TDSInfoToken;
                        LoggingUtilities.WriteLog($"  Client received Info token:");

                        LoggingUtilities.WriteLog($"     Number: {infoToken.Number}");
                        LoggingUtilities.WriteLog($"     State: {infoToken.State}");
                        LoggingUtilities.WriteLog($"     Class: {infoToken.Class}");
                        LoggingUtilities.WriteLog($"     MsgText: {infoToken.MsgText}");
                        LoggingUtilities.WriteLog($"     ServerName: {infoToken.ServerName}");
                        LoggingUtilities.WriteLog($"     ProcName: {infoToken.ProcName}");
                        LoggingUtilities.WriteLog($"     LineNumber: {infoToken.LineNumber}");

                        return null;

                    }
                }
            }
            else
            {
                throw new InvalidOperationException();
            }

            LoggingUtilities.WriteLog($" Login7 response received.");
            return null;

        }

        /// <summary>
        /// Connect to the server.
        /// </summary>
        public async Task<bool> Connect()
        {
            DateTime connectStartTime = DateTime.UtcNow;
            bool preLoginDone = false;
            var originalServerName = this.Server;
            var originalPort = this.Port;

            LoggingUtilities.WriteLog($"Connect initiated (attempt # {++connectionAttempt}).", writeToSummaryLog: true);

            try
            {
                do
                {
                    preLoginDone = false;
                    this.reconnect = false;

                    MeasureDNSResolutionTime();
                    this.Client = new TcpClient(this.Server, this.Port);

                    this.TdsCommunicator = new TDSCommunicator(this.Client.GetStream(), 4096);

                    LoggingUtilities.WriteLog($"  TCP connection open between local {this.Client.Client.LocalEndPoint} and remote {this.Client.Client.RemoteEndPoint}", writeToVerboseLog: false, writeToSummaryLog: true);
                    
                    LoggingUtilities.WriteLog($"  TCP connection open");
                    LoggingUtilities.WriteLog($"   Local endpoint is {this.Client.Client.LocalEndPoint}");
                    LoggingUtilities.WriteLog($"   Remote endpoint is {this.Client.Client.RemoteEndPoint}");
                    connectStartTime = DateTime.UtcNow;
                    this.SendPreLogin();
                    TDSPreLoginPacketData preLoginResponse = (TDSPreLoginPacketData)this.ReceivePreLoginResponse();

                    preLoginDone = true;
                    LoggingUtilities.WriteLog($" PreLogin phase took {(int)(DateTime.UtcNow - connectStartTime).TotalMilliseconds} milliseconds.");

                    if (preLoginResponse.Options.Exists(opt => opt.Type == TDSPreLoginOptionTokenType.FedAuthRequired) && preLoginResponse.FedAuthRequired == TdsPreLoginFedAuthRequiredOption.FedAuthRequired)
                    {
                        this.SendLogin7(false);
                        TdsCommunicator.communicatorState = TDSCommunicatorState.SentLogin7RecordWithoutAuthToken;
                    }
                    else 
                    {
                        this.SendLogin7(true);
                    }

                    Tuple<string, string> res = this.ReceiveLogin7Response();

                    string authority = res.Item1;
                    string resource = res.Item2;
                    string clientID = "cba1fbbe-5d8f-4748-8c97-ba5638ca40ae";

                    string accessToken = await GetSQLAccessTokenUsingUserCredentials(authority, resource, clientID);

                    using (SqlConnection conn = GetSqlConnectionUsingAadAccessToken(accessToken, this.Server, this.Database))
	                {       
		                conn.Open();
		                conn.Close();
                    }
                        
                    if (this.reconnect)
                    {
                        this.Disconnect();
                        LoggingUtilities.AddEmptyLine();
                        LoggingUtilities.WriteLog($" Routing to: {this.Server}:{this.Port}.");
                    }

                    return true;
                }
                while (this.reconnect);
            }
            catch (SocketException socketException)
            {
                LoggingUtilities.WriteLog($" Networking error {socketException.NativeErrorCode} while trying to connect to {this.Server}:{this.Port}.", writeToSummaryLog: true);
                return false;
            }
            catch (Exception ex)
            {
                if (!preLoginDone && DateTime.UtcNow >= connectStartTime.AddSeconds(5))
                {
                    LoggingUtilities.WriteLog($" SNI timeout detected, PreLogin phase was not complete after {(int)(DateTime.UtcNow - connectStartTime).TotalMilliseconds} milliseconds.", writeToSummaryLog: true);
                }
                LoggingUtilities.WriteLog($"Exception:");
                LoggingUtilities.WriteLog($"{ex.Message}");
                if (ex.InnerException != null)
                {
                    LoggingUtilities.WriteLog($"InnerException: {ex.InnerException.Message}");
                }
                //throw ex;
                return false;

            }
            finally
            {
                this.Server = originalServerName;
                this.Port = originalPort;
            }
        }

        private void MeasureDNSResolutionTime()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var addresses = Dns.GetHostAddresses(this.Server);
                stopwatch.Stop();
                var addressListString = string.Join(",", addresses.AsEnumerable());
                LoggingUtilities.WriteLog($"  DNS resolution took {stopwatch.ElapsedMilliseconds} ms, ({addressListString})", writeToSummaryLog: true);
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

        /// <summary>
        /// Disconnect from the server.
        /// </summary>
        public void Disconnect()
        {
            if (this.Client != null)
            {
                LoggingUtilities.WriteLog($" Disconnect initiated.");
                this.Client.Close();
                this.Client = null;
                LoggingUtilities.WriteLog($" Disconnect done.");
            }
        }

        /// <summary>
        /// Gets AAD access token to Azure SQL using user credentials
        /// </summary>
	   public async Task<string> GetSQLAccessTokenUsingUserCredentials(string authority, string resource, string clientID)
	   {
	        try
            {
                AuthenticationContext authContext = new AuthenticationContext(authority);
                UserPasswordCredential uc = new UserPasswordCredential(this.UserID, this.Password);
                AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientID, uc);

                return result.AccessToken;
            }
            catch (AdalException e)
            {
                LoggingUtilities.WriteLog(e.ErrorCode);
                LoggingUtilities.WriteLog(e.Message);
                return null;
            }
        }

        
	    public static SqlConnection GetSqlConnectionUsingAadAccessToken(string accessToken, string serverName, string databaseName = "master")
	    {
		    SqlConnection conn = new SqlConnection(string.Format("Data Source={0};Initial Catalog={1};TrustServerCertificate=true;Pooling=False;", serverName, databaseName));
		    conn.AccessToken = accessToken;

		    return conn;
        }
    
    }
}
