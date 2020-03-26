//  ---------------------------------------------------------------------------
//  <copyright file="BigEndianUtilitiesTests.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.UnitTests.TDS.Utilities
{
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TDSClient.TDS.Utilities;

    [TestClass]
    public class BigEndianUtilitiesTests
    {
        [TestMethod]
        public void ReadUShortFromMemoryStream()
        {
            var stream = new MemoryStream();
            stream.Write(new byte[] { 0x01, 0x23 });
            stream.Seek(0, SeekOrigin.Begin);

            var res = BigEndianUtilities.ReadUShort(stream);

            Assert.AreEqual(0x0123, res);
        }

        [TestMethod]
        public void ReadUIntFromMemoryStream()
        {
            var stream = new MemoryStream();
            stream.Write(new byte[] { 0x01, 0x23, 0x45, 0x67 });
            stream.Seek(0, SeekOrigin.Begin);

            var res = BigEndianUtilities.ReadUInt(stream);

            Assert.AreEqual((uint)0x01234567, res);
        }

        [TestMethod]
        public void ReadULongFromMemoryStream()
        {
            var stream = new MemoryStream();
            stream.Write(new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef });
            stream.Seek(0, SeekOrigin.Begin);

            var res = BigEndianUtilities.ReadULong(stream);

            Assert.AreEqual((ulong)0x0123456789abcdef, res);
        }

        [TestMethod]
        public void ReadByteArrayFromMemoryStream()
        {
            var stream = new MemoryStream();
            stream.Write(new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef });
            stream.Seek(0, SeekOrigin.Begin);

            var res = BigEndianUtilities.ReadByteArray(stream, 8);

            CollectionAssert.AreEqual(new byte[] { 0xef, 0xcd, 0xab, 0x89, 0x67, 0x45, 0x23, 0x01 }, res);
        }

        [TestMethod]
        public void WriteUShortToMemoryStream()
        {
            var stream = new MemoryStream();

            BigEndianUtilities.WriteUShort(stream, 0x0123);

            CollectionAssert.AreEqual(new byte[] { 0x01, 0x23 }, stream.ToArray());
        }

        [TestMethod]
        public void WriteUIntToMemoryStream()
        {
            var stream = new MemoryStream();

            BigEndianUtilities.WriteUInt(stream, 0x01234567);

            CollectionAssert.AreEqual(new byte[] { 0x01, 0x23, 0x45, 0x67 }, stream.ToArray());
        }

        [TestMethod]
        public void WriteULongToMemoryStream()
        {
            var stream = new MemoryStream();

            BigEndianUtilities.WriteULong(stream, 0x0123456789abcdef);

            CollectionAssert.AreEqual(new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef }, stream.ToArray());
        }

        [TestMethod]
        public void WriteByteArrayToMemoryStream()
        {
            var stream = new MemoryStream();

            BigEndianUtilities.WriteByteArray(stream, new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef });

            CollectionAssert.AreEqual(new byte[] { 0xef, 0xcd, 0xab, 0x89, 0x67, 0x45, 0x23, 0x01 }, stream.ToArray());
        }
    }
}
