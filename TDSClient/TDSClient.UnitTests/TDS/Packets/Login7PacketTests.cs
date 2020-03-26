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
    using TDSClient.TDS.Login7;

    [TestClass]
    public class Login7PacketTests
    {
        [TestMethod]
        public void Login7Packing()
        {
            var bytesXML = File.ReadAllBytes("./Assets/PacketData/Login7_Bytes.xml");
            var packetXML = File.ReadAllBytes("./Assets/PacketData/Login7_Object.xml");

            var bytesSerializer = new XmlSerializer(typeof(byte[]));
            var packetSerializer = new XmlSerializer(typeof(TDSLogin7PacketData));

            var bytes = (byte[])bytesSerializer.Deserialize(new MemoryStream(bytesXML));
            var packet = (TDSLogin7PacketData)packetSerializer.Deserialize(new MemoryStream(packetXML));

            var testArray = new byte[bytes.Length];
            packet.Pack(new MemoryStream(testArray));

            Console.WriteLine(string.Join(' ', bytes));
            Console.WriteLine(string.Join(' ', testArray));

            CollectionAssert.AreEqual(bytes, testArray);
        }

        [TestMethod]
        public void Login7Unpacking()
        {
            var bytesXML = File.ReadAllBytes("./Assets/PacketData/Login7_Bytes.xml");
            var packetXML = File.ReadAllBytes("./Assets/PacketData/Login7_Object.xml");

            var bytesSerializer = new XmlSerializer(typeof(byte[]));
            var packetSerializer = new XmlSerializer(typeof(TDSLogin7PacketData));

            var bytes = (byte[])bytesSerializer.Deserialize(new MemoryStream(bytesXML));
            var packet = (TDSLogin7PacketData)packetSerializer.Deserialize(new MemoryStream(packetXML));

            var testPacket = new TDSLogin7PacketData();
            testPacket.Unpack(new MemoryStream(bytes));

            Assert.AreEqual(packet, testPacket);
        }
    }
}