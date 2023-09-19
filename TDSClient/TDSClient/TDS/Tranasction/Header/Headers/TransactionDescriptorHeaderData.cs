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
            TransactionDescriptor = id;
            OutstandingRequestCount = 0;
        }

        public ulong TransactionDescriptor { get; set; }
        public uint OutstandingRequestCount { get; set; } = 0;
        public uint Size => sizeof(ulong) + sizeof(uint);
        public HeaderType HeaderType => HeaderType.TransactionDescriptor;

        public void Pack(MemoryStream stream)
        {
            BigEndianUtilities.WriteULong(stream, TransactionDescriptor);
            BigEndianUtilities.WriteUInt(stream, OutstandingRequestCount);
        }

        public bool Unpack(MemoryStream stream)
        {
            TransactionDescriptor = BigEndianUtilities.ReadULong(stream);
            OutstandingRequestCount = BigEndianUtilities.ReadUInt(stream);
            return true;
        }
    }
}
