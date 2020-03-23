//  ---------------------------------------------------------------------------
//  <copyright file="TDSSQLTestClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Client
{
    using System;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using TDSClient.TDS.Comms;
    using TDSClient.TDS.Header;
    using TDSClient.TDS.Login7;
    using TDSClient.TDS.PreLogin;
    using TDSClient.TDS.Tokens;
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
            this.ServerName = server;
            this.Port = port;
            this.UserID = userID;
            this.Password = password;
            this.Database = database;
            this.EncryptionProtocol = encryptionProtocol;

            LoggingUtilities.WriteLog($" Instantiating TDSSQLTestClient with the following parameters:");

            LoggingUtilities.WriteLog($"     Server: {server}.");
            LoggingUtilities.WriteLog($"     Port: {port}.");
            LoggingUtilities.WriteLog($"     UserID: {userID}.");
            LoggingUtilities.WriteLog($"     Database: {database}.");
        }

        /// <summary>
        /// Gets the Server.
        /// </summary>
        public string Server { get; private set; }

        /// <summary>
        /// Gets the Server Name.
        /// </summary>
        public string ServerName { get; private set; }

        /// <summary>
        /// Gets the Port Number.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets the User ID.
        /// </summary>
        public string UserID { get; private set; }

        /// <summary>
        /// Gets the Password.
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// Gets the Database.
        /// </summary>
        public string Database { get; private set; }

        /// <summary>
        /// Gets the TDS Communicator.
        /// </summary>
        public TDSCommunicator TdsCommunicator { get; private set; }
        
        /// <summary>
        /// Gets the TCP Client.
        /// </summary>
        public TcpClient Client { get; private set; }
        
        /// <summary>
        /// Gets the TDS Client Version.
        /// </summary>
        public TDSClientVersion Version { get; private set; }

        /// <summary>
        /// Gets the Encryption Protocol.
        /// </summary>
        public SslProtocols EncryptionProtocol { get; private set; }

        /// <summary>
        /// Sends PreLogin message to the server.
        /// </summary>
        public void SendPreLogin()
        {
            LoggingUtilities.WriteLog($" SendPreLogin initiated.");
            var tdsMessageBody = new TDSPreLoginPacketData(this.Version);

            tdsMessageBody.AddOption(TDSPreLoginOptionTokenType.Encryption, TDSEncryptionOption.EncryptOff);
            tdsMessageBody.Terminate();

            this.TdsCommunicator.SendTDSMessage(tdsMessageBody);
            LoggingUtilities.WriteLog($" SendPreLogin done.");
        }

        /// <summary>
        /// Sends Login7 message to the server
        /// </summary>
        public void SendLogin7()
        {
            LoggingUtilities.WriteLog($" SendLogin7 initiated.");

            var tdsMessageBody = new TDSLogin7PacketData();

            tdsMessageBody.AddOption("HostName", (ushort)Environment.MachineName.Length, Environment.MachineName);
            tdsMessageBody.AddOption("UserName", (ushort)this.UserID.Length, this.UserID);
            tdsMessageBody.AddOption("ServerName", (ushort)this.ServerName.Length, this.ServerName);
            tdsMessageBody.AddOption("Password", (ushort)this.Password.Length, this.Password);
            tdsMessageBody.AddOption("Database", (ushort)this.Database.Length, this.Database);
            tdsMessageBody.AddOption("IntName", (ushort)"TDSSQLTestClient".Length, "TDSSQLTestClient");

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

            LoggingUtilities.WriteLog($" SendLogin7 done.");
        }

        /// <summary>
        /// Receive PreLogin response from the server.
        /// </summary>
        public void ReceivePreLoginResponse()
        {
            LoggingUtilities.WriteLog($" ReceivePreLoginResponse initiated.");

            if (this.TdsCommunicator.ReceiveTDSMessage() is TDSPreLoginPacketData response)
            {
                if (response.Options.Exists(opt => opt.Type == TDSPreLoginOptionTokenType.Encryption) && response.Encryption == TDSEncryptionOption.EncryptReq)
                {
                    LoggingUtilities.WriteLog($" Server requires encryption, enabling encryption.");
                    this.TdsCommunicator.EnableEncryption(this.Server, this.EncryptionProtocol);
                    LoggingUtilities.WriteLog($" Encryption enabled.");
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

            LoggingUtilities.WriteLog($" ReceivePreLoginResponse done.");
        }

        /// <summary>
        /// Receive Login7 response from the server.
        /// </summary>
        public void ReceiveLogin7Response()
        {
            LoggingUtilities.WriteLog($" ReceiveLogin7Response initiated.");

            if (this.TdsCommunicator.ReceiveTDSMessage() is TDSTokenStreamPacketData response)
            {
                foreach (var token in response.Tokens)
                {
                    if (token is TDSEnvChangeToken)
                    {
                        var envChangeToken = token as TDSEnvChangeToken;
                        if (envChangeToken.Type == Tokens.EnvChange.TDSEnvChangeType.Routing)
                        {
                            LoggingUtilities.WriteLog($" Client recieved EnvChange routing token, client is being routed.");
                            this.Server = envChangeToken.Values["AlternateServer"];
                            this.ServerName = this.Server;
                            this.Port = int.Parse(envChangeToken.Values["ProtocolProperty"]);
                            this.reconnect = true;
                        }
                    }
                    else if (token is TDSErrorToken)
                    {
                        var errorToken = token as TDSErrorToken;
                        LoggingUtilities.WriteLog($" Client recieved Error token:");

                        LoggingUtilities.WriteLog($"     Number: {errorToken.Number}");
                        LoggingUtilities.WriteLog($"     State: {errorToken.State}");
                        LoggingUtilities.WriteLog($"     Class: {errorToken.Class}");
                        LoggingUtilities.WriteLog($"     MsgText: {errorToken.MsgText}");
                        LoggingUtilities.WriteLog($"     ServerName: {errorToken.ServerName}");
                        LoggingUtilities.WriteLog($"     ProcName: {errorToken.ProcName}");
                        LoggingUtilities.WriteLog($"     LineNumber: {errorToken.LineNumber}");

                        if (errorToken.Number == 18456)
                        {
                            throw new Exception("Login failure.");
                        }
                    }
                }
            }
            else
            {
                throw new InvalidOperationException();
            }

            LoggingUtilities.WriteLog($" ReceiveLogin7Response done.");
        }

        /// <summary>
        /// Connect to the server.
        /// </summary>
        public void Connect()
        {
            LoggingUtilities.WriteLog($" Connect initiated.");
            do
            {
                this.reconnect = false;
                this.Client = new TcpClient(this.Server, this.Port);
                this.TdsCommunicator = new TDSCommunicator(this.Client.GetStream(), 4096);
                this.SendPreLogin();
                this.ReceivePreLoginResponse();
                this.SendLogin7();
                this.ReceiveLogin7Response();

                if (this.reconnect)
                {
                    this.Disconnect();
                    LoggingUtilities.WriteLog($" Routing to: {this.Server}:{this.Port}.");
                }
            } 
            while (this.reconnect);

            LoggingUtilities.WriteLog($" Connect done.");
        }

        /// <summary>
        /// Disconnect from the server.
        /// </summary>
        public void Disconnect()
        {
            LoggingUtilities.WriteLog($" Disconnect initiated.");
            this.Client.Close();
            this.Client = null;
            LoggingUtilities.WriteLog($" Disconnect done.");
        }
    }
}
