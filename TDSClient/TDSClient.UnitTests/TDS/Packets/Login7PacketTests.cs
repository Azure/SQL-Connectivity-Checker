//  ---------------------------------------------------------------------------
//  <copyright file="Login7PacketTests.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.UnitTests.TDS.Packets
{
    using System;
    using System.IO;
    using System.Xml.Serialization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using TDSClient.TDS.Login7;

    [TestClass]
    public class Login7PacketTests
    {
        [TestMethod]
        public void Login7Packing()
        {
            var bytesJSON = File.ReadAllText("./Assets/PacketData/Login7_Bytes.json");
            var packetJSON = File.ReadAllText("./Assets/PacketData/Login7_Object.json");

            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var bytes = JsonConvert.DeserializeObject<byte[]>(bytesJSON, settings);
            var packet = JsonConvert.DeserializeObject<TDSLogin7PacketData>(packetJSON, settings);

            var testArray = new byte[bytes.Length];
            packet.Pack(new MemoryStream(testArray));

            CollectionAssert.AreEqual(bytes, testArray);
        }

        [TestMethod]
        public void Login7Unpacking()
        {
            var bytesJSON = File.ReadAllText("./Assets/PacketData/Login7_Bytes.json");
            var packetJSON = File.ReadAllText("./Assets/PacketData/Login7_Object.json");

            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var bytes = JsonConvert.DeserializeObject<byte[]>(bytesJSON, settings);
            var packet = JsonConvert.DeserializeObject<TDSLogin7PacketData>(packetJSON, settings);

            var testPacket = new TDSLogin7PacketData();
            testPacket.Unpack(new MemoryStream(bytes));
            var a = JsonConvert.SerializeObject(testPacket, settings);
            Assert.AreEqual(packet, testPacket);
        }

        [TestMethod]
        public void Login7PacketCreation()
        {
            var bytesJSON = File.ReadAllText("./Assets/PacketData/Login7_Bytes.json");
            var packetJSON = File.ReadAllText("./Assets/PacketData/Login7_Object.json");

            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var bytes = JsonConvert.DeserializeObject<byte[]>(bytesJSON, settings);
            var packet = JsonConvert.DeserializeObject<TDSLogin7PacketData>(packetJSON, settings);

            var testPacket = new TDSLogin7PacketData();

            testPacket.OptionFlags1.ByteOrder = TDSLogin7OptionFlags1ByteOrder.OrderX86;
            testPacket.OptionFlags1.Char = TDSLogin7OptionFlags1Char.CharsetASCII;
            testPacket.OptionFlags1.Database = TDSLogin7OptionFlags1Database.InitDBFatal;
            testPacket.OptionFlags1.DumpLoad = TDSLogin7OptionFlags1DumpLoad.DumploadOn;
            testPacket.OptionFlags1.Float = TDSLogin7OptionFlags1Float.FloatIEEE754;
            testPacket.OptionFlags1.SetLang = TDSLogin7OptionFlags1SetLang.SetLangOn;
            testPacket.OptionFlags1.UseDB = TDSLogin7OptionFlags1UseDB.UseDBOn;

            testPacket.OptionFlags2.IntSecurity = TDSLogin7OptionFlags2IntSecurity.IntegratedSecurityOff;
            testPacket.OptionFlags2.Language = TDSLogin7OptionFlags2Language.InitLangFatal;
            testPacket.OptionFlags2.ODBC = TDSLogin7OptionFlags2ODBC.OdbcOn;
            testPacket.OptionFlags2.UserType = TDSLogin7OptionFlags2UserType.UserNormal;

            testPacket.OptionFlags3.ChangePassword = TDSLogin7OptionFlags3ChangePassword.NoChangeRequest;
            testPacket.OptionFlags3.Extension = TDSLogin7OptionFlags3Extension.DoesntExist;
            testPacket.OptionFlags3.SendYukonBinaryXML = TDSLogin7OptionFlags3SendYukonBinaryXML.Off;
            testPacket.OptionFlags3.UnknownCollationHandling = TDSLogin7OptionFlags3UnknownCollationHandling.Off;
            testPacket.OptionFlags3.UserInstanceProcess = TDSLogin7OptionFlags3UserInstanceProcess.DontRequestSeparateProcess;

            testPacket.TypeFlags.OLEDB = TDSLogin7TypeFlagsOLEDB.Off;
            testPacket.TypeFlags.ReadOnlyIntent = TDSLogin7TypeFlagsReadOnlyIntent.Off;
            testPacket.TypeFlags.SQLType = TDSLogin7TypeFlagsSQLType.DFLT;

            testPacket.TDSVersion = 0x72090002;
            testPacket.ClientLCID = 0x00000409;
            testPacket.ClientPID = 0x00000100;
            testPacket.ClientProgVer = 0x7000000;
            testPacket.ClientTimeZone = 0x000001e0;
            testPacket.ConnectionID = 0x00000000;

            testPacket.AddOption("HostName", "skostov1");
            testPacket.AddOption("CltIntName", "ODBC");
            testPacket.AddOption("AppName", "OSQL-32");
            testPacket.AddOption("UserName", "sa");

            testPacket.ClientID = new byte[] { 0x00, 0x50, 0x8b, 0xe2, 0xb7, 0x8f };

            var packedBytes = new byte[testPacket.Length];
            testPacket.Pack(new MemoryStream(packedBytes));

            Assert.AreEqual(packet, testPacket);
            CollectionAssert.AreEqual(bytes, packedBytes);
        }
    }
}