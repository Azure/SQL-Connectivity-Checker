namespace TDSClient.UnitTests.TDS.Utilities
{
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TDSClient.TDS.Utilities;

    [TestClass]
    public class LittleEndianUtilitiesTests
    {
        [TestMethod]
        public void ReadUShortFromMemoryStream()
        {
            var stream = new MemoryStream();
            stream.Write(new byte[] { 0x01, 0x23 });
            stream.Seek(0, SeekOrigin.Begin);

            var res = LittleEndianUtilities.ReadUShort(stream);

            Assert.AreEqual(0x2301, res);
        }

        [TestMethod]
        public void ReadUIntFromMemoryStream()
        {
            var stream = new MemoryStream();
            stream.Write(new byte[] { 0x01, 0x23, 0x45, 0x67 });
            stream.Seek(0, SeekOrigin.Begin);

            var res = LittleEndianUtilities.ReadUInt(stream);

            Assert.AreEqual((uint)0x67452301, res);
        }

        [TestMethod]
        public void WriteUShortToMemoryStream()
        {
            var stream = new MemoryStream();

            LittleEndianUtilities.WriteUShort(stream, 0x0123);

            CollectionAssert.AreEqual(new byte[] { 0x23, 0x01 }, stream.ToArray());
        }

        [TestMethod]
        public void WriteUIntToMemoryStream()
        {
            var stream = new MemoryStream();

            LittleEndianUtilities.WriteUInt(stream, 0x01234567);

            CollectionAssert.AreEqual(new byte[] { 0x67, 0x45, 0x23, 0x01 }, stream.ToArray());
        }
    }
}
