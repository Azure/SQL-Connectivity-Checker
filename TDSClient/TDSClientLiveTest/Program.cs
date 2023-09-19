using System;
using System.Security.Authentication;
using System.Text;
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

        static void Main(string[] args)
        {
            TDSSQLTestClient tdsClient = new TDSSQLTestClient(Server, Port, Username, Password, Database, SslProtocols.None);
            TDSClient.TDS.Utilities.LoggingUtilities.SetVerboseLog(Console.Out);

            for (int i = 0; i < 1; i++)
            {
                tdsClient.Connect();
                tdsClient.Query("select 1");
                tdsClient.Disconnect();
                Console.WriteLine();
                Thread.Sleep(1000);
            }
        }
    }
}
