//  ---------------------------------------------------------------------------
//  <copyright file="LoginResponsePacketTests.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.UnitTests.TDS.Packets
{
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using TDSClient.TDS.Tokens;

    [TestClass]
    public class LoginResponsePacketTests
    {
        [TestMethod]
        public void LoginResponseUnpacking()
        {   
            var bytesJSON = File.ReadAllText("./Assets/PacketData/LoginResponse_Bytes.json");
            var packetJSON = File.ReadAllText("./Assets/PacketData/LoginResponse_Object.json");

            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var bytes = JsonConvert.DeserializeObject<byte[]>(bytesJSON, settings);
            var packet = JsonConvert.DeserializeObject<TDSTokenStreamPacketData>(packetJSON, settings);

            var testPacket = new TDSTokenStreamPacketData();
            testPacket.Unpack(new MemoryStream(bytes));

            Assert.AreEqual(packet, testPacket);
        }
    }
}
