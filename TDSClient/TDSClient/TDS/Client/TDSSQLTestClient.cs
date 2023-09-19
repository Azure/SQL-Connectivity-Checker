//  ---------------------------------------------------------------------------
//  <copyright file="TDSSQLTestClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Threading;
    using TDSClient.TDS.Comms;
    using TDSClient.TDS.Header;
    using TDSClient.TDS.Login7;
    using TDSClient.TDS.PreLogin;
    using TDSClient.TDS.Query;
    using TDSClient.TDS.Tokens;
    using TDSClient.TDS.Tokens.Cols;
    using TDSClient.TDS.Tranasction.Header.Headers;
    using TDSClient.TDS.Tranasction.Headers;
    using TDSClient.TDS.Utilities;

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
        /// <param name="userID">Used ID</param>
        /// <param name="password">User password</param>
        /// <param name="database">Database to connect to</param>
        /// <param name="encryptionProtocol">Encryption Protocol</param>
        public TDSSQLTestClient(string server, int port, string userID, string password, string database, SslProtocols encryptionProtocol = SslProtocols.Tls12)
        {
            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(database))
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

            LoggingUtilities.WriteLog($" Instantiating TDSSQLTestClient with the following parameters:");

            LoggingUtilities.WriteLog($"     Server: {server}.");
            LoggingUtilities.WriteLog($"     Port: {port}.");
            LoggingUtilities.WriteLog($"     UserID: {userID}.");
            LoggingUtilities.WriteLog($"     Database: {database}.");
        }

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
            tdsMessageBody.Terminate();

            this.TdsCommunicator.SendTDSMessage(tdsMessageBody);
            LoggingUtilities.WriteLog($" PreLogin message sent.");
        }

        /// <summary>
        /// Sends Login7 message to the server
        /// </summary>
        public void SendLogin7()
        {
            LoggingUtilities.AddEmptyLine();
            LoggingUtilities.WriteLog($" Building Login7 message.");

            var tdsMessageBody = new TDSLogin7PacketData();

            tdsMessageBody.AddOption("HostName", Environment.MachineName);
            tdsMessageBody.AddOption("UserName", this.UserID);
            tdsMessageBody.AddOption("ServerName", this.Server);
            tdsMessageBody.AddOption("Password", this.Password);
            tdsMessageBody.AddOption("Database", this.Database);
            tdsMessageBody.AddOption("CltIntName", "TDSSQLTestClient");

            tdsMessageBody.OptionFlags1.Char = TDSLogin7OptionFlags1Char.CharsetASCII;
            tdsMessageBody.OptionFlags1.Database = TDSLogin7OptionFlags1Database.InitDBFatal;
            tdsMessageBody.OptionFlags1.DumpLoad = TDSLogin7OptionFlags1DumpLoad.DumploadOn;
            tdsMessageBody.OptionFlags1.Float = TDSLogin7OptionFlags1Float.FloatIEEE754;
            tdsMessageBody.OptionFlags1.SetLang = TDSLogin7OptionFlags1SetLang.SetLangOn;
            tdsMessageBody.OptionFlags1.ByteOrder = TDSLogin7OptionFlags1ByteOrder.OrderX86;
            tdsMessageBody.OptionFlags1.UseDB = TDSLogin7OptionFlags1UseDB.UseDBOff;

            tdsMessageBody.OptionFlags2.Language = TDSLogin7OptionFlags2Language.InitLangFatal;
            tdsMessageBody.OptionFlags2.ODBC = TDSLogin7OptionFlags2ODBC.OdbcOn;
            tdsMessageBody.OptionFlags2.UserType = TDSLogin7OptionFlags2UserType.UserNormal;

            tdsMessageBody.OptionFlags3.ChangePassword = TDSLogin7OptionFlags3ChangePassword.NoChangeRequest;
            tdsMessageBody.OptionFlags3.UserInstanceProcess = TDSLogin7OptionFlags3UserInstanceProcess.DontRequestSeparateProcess;
            tdsMessageBody.OptionFlags3.UnknownCollationHandling = TDSLogin7OptionFlags3UnknownCollationHandling.On;
            tdsMessageBody.OptionFlags3.Extension = TDSLogin7OptionFlags3Extension.DoesntExist;

            tdsMessageBody.TypeFlags.OLEDB = TDSLogin7TypeFlagsOLEDB.On;
            tdsMessageBody.TypeFlags.SQLType = TDSLogin7TypeFlagsSQLType.DFLT;
            tdsMessageBody.TypeFlags.ReadOnlyIntent = TDSLogin7TypeFlagsReadOnlyIntent.On;

            this.TdsCommunicator.SendTDSMessage(tdsMessageBody);

            LoggingUtilities.WriteLog($" Login7 message sent.");
        }

        /// <summary>
        /// Receive PreLogin response from the server.
        /// </summary>
        public void ReceivePreLoginResponse()
        {
            LoggingUtilities.AddEmptyLine();
            LoggingUtilities.WriteLog($" Waiting for PreLogin response.");

            if (this.TdsCommunicator.ReceiveTDSMessage() is TDSPreLoginPacketData response)
            {
                if (response.Options.Exists(opt => opt.Type == TDSPreLoginOptionTokenType.Encryption) && response.Encryption == TDSEncryptionOption.EncryptReq)
                {
                    LoggingUtilities.WriteLog($"  Server requires encryption, enabling encryption.");
                    this.TdsCommunicator.EnableEncryption(this.Server, this.EncryptionProtocol);
                    LoggingUtilities.WriteLog($"  Encryption enabled.");
                }

                if (response.Options.Exists(opt => opt.Type == TDSPreLoginOptionTokenType.FedAuthRequired) && response.FedAuthRequired == true)
                {
                    throw new NotSupportedException("FedAuth is being requested but the client doesn't support FedAuth.");
                }
            }
            else
            {
                throw new InvalidOperationException();
            }

            LoggingUtilities.WriteLog($" PreLogin response received.");
        }

        /// <summary>
        /// Receive Login7 response from the server.
        /// </summary>
        public void ReceiveLogin7Response()
        {
            LoggingUtilities.AddEmptyLine();
            LoggingUtilities.WriteLog($" Waiting for Login7 response.");

            if (this.TdsCommunicator.ReceiveTDSMessage() is TDSTokenStreamPacketData response)
            {
                foreach (var token in response.Tokens)
                {
                    PrintTdsToken(token);
                }
            }
            else
            {
                throw new InvalidOperationException();
            }

            LoggingUtilities.WriteLog($" Login7 response received.");
        }

        private void PrintTdsToken(TDSToken token)
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

                if (errorToken.Number == 18456)
                {
                    throw new Exception("Login failure.");
                }
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
            }
            else if (token is TDSColMetadataToken colMetadataToken)
            {
                LoggingUtilities.WriteLog($"  Client Column Metadata Info token:");

                LoggingUtilities.WriteLog($"     Columns: {colMetadataToken.Count}");
                for (int i = 0; i < colMetadataToken.Metadata.Length; i++)
                {
                    var metadata = colMetadataToken.Metadata[i];
                    LoggingUtilities.WriteLog($"        Index: {i}");
                    LoggingUtilities.WriteLog($"        Name: {metadata.ColumnName}");
                    LoggingUtilities.WriteLog($"        Type: {metadata.Type.Type}");
                    LoggingUtilities.WriteLog("");
                }
            }
            else if (token is TDSRowToken rowToken)
            {
                LoggingUtilities.WriteLog($"  Client Row data token:");
                for(int i = 0; i < rowToken.Values.Length; i++)
                {
                    LoggingUtilities.WriteLog($"    Row [{i}]: {rowToken.Values[i]?.ToString() ?? "NUll"}");
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
                    this.ReceivePreLoginResponse();
                    preLoginDone = true;
                    LoggingUtilities.WriteLog($" PreLogin phase took {(int)(DateTime.UtcNow - connectStartTime).TotalMilliseconds} milliseconds.");
                    this.SendLogin7();
                    this.ReceiveLogin7Response();

                    if (this.reconnect)
                    {
                        this.Disconnect();
                        LoggingUtilities.AddEmptyLine();
                        LoggingUtilities.WriteLog($" Routing to: {this.Server}:{this.Port}.");
                    }
                }
                while (this.reconnect);

                LoggingUtilities.WriteLog($" Connect done.", writeToSummaryLog: true);
            }
            catch (SocketException socketException)
            {
                LoggingUtilities.WriteLog($" Networking error {socketException.NativeErrorCode} while trying to connect to {this.Server}:{this.Port}.", writeToSummaryLog: true);
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
            }
            finally
            {
                this.Server = originalServerName;
                this.Port = originalPort;
            }
        }

        public int queryCount = 0;
        public void Query(string query)
        {
            var id = Interlocked.Increment(ref queryCount);
            try
            {
                var request = new TDSSqlBatchPacketData(query);
                var header = new SqlHeaderPacketData(new TransactionDescriptorHeaderData((uint)id));
                request.AllHeaders.Headers.Add(header);

                TdsCommunicator.SendTDSMessage(request);
               
                if (this.TdsCommunicator.ReceiveTDSMessage() is TDSTokenStreamPacketData response)
                {
                    foreach (var token in response.Tokens)
                    {
                        PrintTdsToken(token);
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            catch(Exception ex)
            {
                LoggingUtilities.WriteLog($"Exception:");
                LoggingUtilities.WriteLog($"{ex.Message}");
                if (ex.InnerException != null)
                {
                    LoggingUtilities.WriteLog($"InnerException: {ex.InnerException.Message}");
                }
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
    }
}
