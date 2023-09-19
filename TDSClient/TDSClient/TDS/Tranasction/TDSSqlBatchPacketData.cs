using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TDSClient.TDS.Client;
using TDSClient.TDS.Interfaces;
using TDSClient.TDS.PreLogin;
using TDSClient.TDS.Tranasction.Headers;
using TDSClient.TDS.Utilities;

namespace TDSClient.TDS.Query
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class TDSSqlBatchPacketData : ITDSPacketData, IEquatable<TDSSqlBatchPacketData>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {

        private string _query = string.Empty;

        public TDSSqlBatchPacketData(string query)
        {
            _query = query;
        }

        public SqlAllHeadersPacketData AllHeaders { get; set; } = new SqlAllHeadersPacketData();

        public string SqlText => _query;

        public bool Equals(TDSSqlBatchPacketData other)
        {
            throw new NotImplementedException();
        }

        public ushort Length() => (ushort)(AllHeaders.TotalLength + SqlText.Length * sizeof(char) + sizeof(ulong));

        public void Pack(MemoryStream stream)
        {
            AllHeaders.Pack(stream);
            //Enclave can be empty stream
            //BigEndianUtilities.WriteULong(stream, 0UL);
            BigEndianUtilities.WriteUnicodeStream(stream, SqlText);
        }

        public bool Unpack(MemoryStream stream)
        {
            //TODO: Get original length from tds packet size
            throw new NotImplementedException();
        }
    }
}
