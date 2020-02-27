using System;
using System.Net.Sockets;
using TDSClient.TDS.Header;
using TDSClient.TDS.PreLogin;
using TDSClient.TDS.Comms;
using TDSClient.TDS.Login7;
using TDSClient.TDS.Tokens;
using TDSClient.TDS.Utilities;

namespace TDSClient.TDS.Client
{
    public class TDSSQLTestClient
    {
        public string Server { get; private set; }
        public string ServerName { get; private set; }
        public int Port { get; private set; }
        public string UserID { get; private set; }
        public string Password { get; private set; }
        public string Database { get; private set; }

        public TDSCommunicator TdsCommunicator { get; private set; }
        public TcpClient Client { get; private set; }
        public TDSClientVersion Version { get; private set; }

        private bool Reconnect;

        public TDSSQLTestClient(string server, int port, string userID, string password, string database)
        {
            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(database))
            {
                throw new ArgumentNullException();
            }

            Client = null;
            Version = new TDSClientVersion(1, 0, 0, 0);
            Server = server;
            ServerName = server;
            Port = port;
            UserID = userID;
            Password = password;
            Database = database;

            LoggingUtilities.WriteLog($" Instantiating TDSSQLTestClient with the following parameters:");

            LoggingUtilities.WriteLog($"     Server: {server}.");
            LoggingUtilities.WriteLog($"     Port: {port}.");
            LoggingUtilities.WriteLog($"     UserID: {userID}.");
            LoggingUtilities.WriteLog($"     Database: {database}.");
        }

        public void SendPreLogin()
        {
            LoggingUtilities.WriteLog($" SendPreLogin initiated.");
            var tdsMessageBody = new TDSPreLoginPacketData(Version);
            
            tdsMessageBody.AddOption(TDSPreLoginOptionTokenType.Encryption, TDSEncryptionOption.EncryptOff);
            tdsMessageBody.Terminate();

            TdsCommunicator.SendTDSMessage(tdsMessageBody);
            LoggingUtilities.WriteLog($" SendPreLogin done.");
        }

        public void SendLogin7()
        {
            LoggingUtilities.WriteLog($" SendLogin7 initiated.");

            var tdsMessageBody = new TDSLogin7PacketData();

            tdsMessageBody.AddOption("HostName", (ushort)Environment.MachineName.Length, Environment.MachineName);
            tdsMessageBody.AddOption("UserName", (ushort)UserID.Length, UserID);
            tdsMessageBody.AddOption("ServerName", (ushort)ServerName.Length, ServerName);
            tdsMessageBody.AddOption("Password", (ushort)Password.Length, Password);
            tdsMessageBody.AddOption("Database", (ushort)Database.Length, Database);
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

            TdsCommunicator.SendTDSMessage(tdsMessageBody);

            LoggingUtilities.WriteLog($" SendLogin7 done.");
        }

        public void ReceiveLogin7Response()
        {
            LoggingUtilities.WriteLog($" ReceiveLogin7Response initiated.");

            if (TdsCommunicator.ReceiveTDSMessage() is TDSTokenStreamPacketData response)
            {
                foreach (var token in response.Tokens)
                {
                    if (token is TDSEnvChangeToken)
                    {
                        var envChangeToken = token as TDSEnvChangeToken;
                        if (envChangeToken.Type == Tokens.EnvChange.TDSEnvChangeType.Routing)
                        {
                            LoggingUtilities.WriteLog($" Client recieved EnvChange routing token, client is being routed.");
                            Server = envChangeToken.Values["AlternateServer"];
                            Port = int.Parse(envChangeToken.Values["ProtocolProperty"]);
                            Reconnect = true;
                        }
                    } else if (token is TDSErrorToken)
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

        public void ReceivePreLoginResponse()
        {
            LoggingUtilities.WriteLog($" ReceivePreLoginResponse initiated.");

            if (TdsCommunicator.ReceiveTDSMessage() is TDSPreLoginPacketData response)
            {
                if (response.Options.Exists(opt => opt.Type == TDSPreLoginOptionTokenType.Encryption) && response.Encryption == TDSEncryptionOption.EncryptReq)
                {
                    LoggingUtilities.WriteLog($" Server requires encryption, enabling encryption.");
                    TdsCommunicator.EnableEncryption(Server);
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

        public void Connect()
        {
            LoggingUtilities.WriteLog($" Connect initiated.");
            do
            {
                Reconnect = false;
                Client = new TcpClient(Server, Port);
                TdsCommunicator = new TDSCommunicator(Client.GetStream(), 4096);
                SendPreLogin();
                ReceivePreLoginResponse();
                SendLogin7();
                ReceiveLogin7Response();
                
                if (Reconnect)
                {
                    Disconnect();
                    LoggingUtilities.WriteLog($" Routing to: {Server}:{Port}.");
                }
            } while (Reconnect);

            LoggingUtilities.WriteLog($" Connect done.");
        }

        public void Disconnect()
        {
            LoggingUtilities.WriteLog($" Disconnect initiated.");
            Client.Close();
            Client = null;
            LoggingUtilities.WriteLog($" Disconnect done.");
        }

    }
}
