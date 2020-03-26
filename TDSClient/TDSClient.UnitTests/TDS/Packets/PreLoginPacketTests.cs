namespace TDSClient.UnitTests.TDS.Packets
{
    using System.IO;
    using System.Xml.Serialization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TDSClient.TDS.PreLogin;

    [TestClass]
    public class PreLoginPacketTests
    {
        [TestMethod]
        public void PreLoginPacking()
        {
            var bytesXML = File.ReadAllBytes("./Assets/PacketData/PreLogin_Bytes.xml");
            var packetXML = File.ReadAllBytes("./Assets/PacketData/PreLogin_Object.xml");

            var bytesSerializer = new XmlSerializer(typeof(byte[]));
            var packetSerializer = new XmlSerializer(typeof(TDSPreLoginPacketData));

            var bytes = (byte[])bytesSerializer.Deserialize(new MemoryStream(bytesXML));
            var packet = (TDSPreLoginPacketData)packetSerializer.Deserialize(new MemoryStream(packetXML));

            var testArray = new byte[bytes.Length];
            packet.Pack(new MemoryStream(testArray));

            CollectionAssert.AreEqual(bytes, testArray);
        }

        [TestMethod]
        public void PreLoginUnpacking()
        {
            var bytesXML = File.ReadAllBytes("./Assets/PacketData/PreLogin_Bytes.xml");
            var packetXML = File.ReadAllBytes("./Assets/PacketData/PreLogin_Object.xml");

            var bytesSerializer = new XmlSerializer(typeof(byte[]));
            var packetSerializer = new XmlSerializer(typeof(TDSPreLoginPacketData));

            var bytes = (byte[])bytesSerializer.Deserialize(new MemoryStream(bytesXML));
            var packet = (TDSPreLoginPacketData)packetSerializer.Deserialize(new MemoryStream(packetXML));

            var testPacket = new TDSPreLoginPacketData();
            testPacket.Unpack(new MemoryStream(bytes));

            Assert.AreEqual(packet, testPacket);
        }
    }
}
