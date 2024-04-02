//  ---------------------------------------------------------------------------
//  <copyright file="PreLoginPacketTests.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.UnitTests.TDS.Packets
{
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using TDSClient.TDS.PreLogin;

    [TestClass]
    public class PreLoginPacketTests
    {
        [TestMethod]
        public void PreLoginPacking()
        {
            var bytesJSON = File.ReadAllText("./Assets/PacketData/PreLogin_Bytes.json");
            var packetJSON = File.ReadAllText("./Assets/PacketData/PreLogin_Object.json");

            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var bytes = JsonConvert.DeserializeObject<byte[]>(bytesJSON, settings);
            var packet = JsonConvert.DeserializeObject<TDSPreLoginPacketData>(packetJSON, settings);

            var testArray = new byte[bytes.Length];
            packet.Pack(new MemoryStream(testArray));

            CollectionAssert.AreEqual(bytes, testArray);
        }

        [TestMethod]
        public void PreLoginUnpacking()
        {
            var bytesJSON = File.ReadAllText("./Assets/PacketData/PreLogin_Bytes.json");
            var packetJSON = File.ReadAllText("./Assets/PacketData/PreLogin_Object.json");

            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var bytes = JsonConvert.DeserializeObject<byte[]>(bytesJSON, settings);
            var packet = JsonConvert.DeserializeObject<TDSPreLoginPacketData>(packetJSON, settings);

            var testPacket = new TDSPreLoginPacketData();
            testPacket.Unpack(new MemoryStream(bytes));

            Assert.AreEqual(packet, testPacket);
        }
    }
}
