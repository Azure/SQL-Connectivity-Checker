using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TDSClient.TDS.Interfaces;
using TDSClient.TDS.Tranasction.Headers;
using TDSClient.TDS.Utilities;

namespace TDSClient.TDS.Tranasction.Header.Headers
{
    public class TransactionDescriptorHeaderData : ISqlHeaderData
    {
        internal TransactionDescriptorHeaderData()
        {

        }

        public TransactionDescriptorHeaderData(uint id)
        {
            TransactionDescriptor = 0;
            OutstandingRequestCount = id;
        }

        public ulong TransactionDescriptor { get; set; }
        public uint OutstandingRequestCount { get; set; } = 0;
        public uint Size => sizeof(ulong) + sizeof(uint);
        public HeaderType HeaderType => HeaderType.TransactionDescriptor;

        public void Pack(MemoryStream stream)
        {
            BigEndianUtilities.WriteULongLE(stream, TransactionDescriptor);
            BigEndianUtilities.WriteUIntLE(stream, OutstandingRequestCount);
        }

        public bool Unpack(MemoryStream stream)
        {
            TransactionDescriptor = BigEndianUtilities.ReadULong(stream);
            OutstandingRequestCount = BigEndianUtilities.ReadUInt(stream);
            return true;
        }
    }
}
