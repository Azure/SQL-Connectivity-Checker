using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TDSClient.TDS.Interfaces;
using TDSClient.TDS.Utilities;

namespace TDSClient.TDS.Tranasction.Headers
{
    public class SqlAllHeadersPacketData : IPackageable
    {
        public List<SqlHeaderPacketData> Headers { get; set; } = new List<SqlHeaderPacketData>();
        public uint TotalLength => sizeof(uint) + (uint)Headers.Sum(x => x.Size);

        public void Pack(MemoryStream stream)
        {
            BigEndianUtilities.WriteUInt(stream, TotalLength);
            foreach (var header in Headers)
            {
                header.Pack(stream);
            }
        }

        public bool Unpack(MemoryStream stream)
        {
            throw new NotImplementedException();
        }
    }
}
