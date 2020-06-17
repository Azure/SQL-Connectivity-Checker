using System;
using TDSClient.TDS.Client;

namespace TDSClientLiveTest
{
    class Program
    {
        public static string Server = "";
        public static int Port = 3342;
        public static string Username = "";
        public static string Password = "";
        public static string Database = "";


        static void Main(string[] args)
        {
            TDSSQLTestClient tdsClient = new TDSSQLTestClient(Server, Port, Username, Password, Database);
            TDSClient.TDS.Utilities.LoggingUtilities.SetVerboseLog(Console.Out);
            tdsClient.Connect();
            tdsClient.Disconnect();
        }
    }
}
