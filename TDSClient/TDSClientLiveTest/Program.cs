using System;
using System.Threading;
using TDSClient.TDS.Client;

namespace TDSClientLiveTest
{
    class Program
    {
        public static string Server = ".database.windows.net";
        public static string Database = "";
        public static int Port = 1433;
        public static string Username = "AzSQLConnCheckerUser";
        public static string Password = "AzSQLConnCheckerPassword";
        public static string AuthenticationType = "SQL Server Authentication";
        public static string AuthenticationLibrary = "MSAL";

        static void Main(string[] args)
        {
            TDSSQLTestClient tdsClient = new TDSSQLTestClient(Server, Port, AuthenticationType, Username, Password, Database);
            TDSClient.TDS.Utilities.LoggingUtilities.SetVerboseLog(Console.Out);
            //TDSClient.TDS.Utilities.LoggingUtilities.SetSummaryLog(Console.Out);

            for (int i = 0; i < 1; i++)
            {
                tdsClient.Connect();
                tdsClient.Disconnect();
                Console.WriteLine();
                Thread.Sleep(1000);
            }
        }
    }
}
