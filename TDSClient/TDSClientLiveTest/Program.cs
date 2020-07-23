using System;
using TDSClient.TDS.Client;
using TDSClient.TDS.Header;

namespace TDSClientLiveTest
{
    class Program
    {
        public static string Server = "localhost";
        public static int Port = 1433;
        public static string Username = "admin";
        public static string Password = "admin";
        public static string Database = "master";
        public static bool TrustServerCertificate = true;
        public static TDSEncryptionOption EncryptionOption = TDSEncryptionOption.EncryptOn;
        
        static void Main(string[] args)
        {
            TDSSQLTestClient tdsClient = new TDSSQLTestClient(Server, Port, Username, Password, Database, TrustServerCertificate, EncryptionOption);
            TDSClient.TDS.Utilities.LoggingUtilities.SetVerboseLog(Console.Out);
            tdsClient.Connect();
            tdsClient.Disconnect();
        }
    }
}