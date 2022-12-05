using System;
using System.Threading;
using TDSClient.TDS.Client;

namespace TDSClientLiveTest
{
    class Program
    {
        public static string Server = "";
        public static int Port = 1433;
        public static string Username = "";
        public static string Password = "";
        public static string Database = "";
        public static string AuthenticationType = "Azure Active Directory Password";


        static void Main(string[] args)
        {
            TDSSQLTestClient tdsClient = new TDSSQLTestClient(Server, Port, AuthenticationType, Username, Password, Database);
            TDSClient.TDS.Utilities.LoggingUtilities.SetVerboseLog(Console.Out);
            //TDSClient.TDS.Utilities.LoggingUtilities.SetSummaryLog(Console.Out);

            for (int i = 0; i < 10; i++)
            {
                tdsClient.Connect();
                tdsClient.Disconnect();
                Console.WriteLine();
                Thread.Sleep(1000);
            }
        }
    }
}
