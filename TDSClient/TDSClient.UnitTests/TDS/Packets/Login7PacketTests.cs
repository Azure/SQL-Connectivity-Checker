//  ---------------------------------------------------------------------------
//  <copyright file="Login7PacketTests.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.UnitTests.TDS.Packets
{
    using System.IO;
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

            var testPacket = new TDSLogin7PacketData("skostov1", "OSQL-32", "test-server", "test-database");
            testPacket.Unpack(new MemoryStream(bytes));
            var a = JsonConvert.SerializeObject(testPacket, settings);
            Assert.AreEqual(packet, testPacket);
        }

        [TestMethod]
        public void Login7PacketCreation()
        {
            // TODO: Add test.
        }
    }
}