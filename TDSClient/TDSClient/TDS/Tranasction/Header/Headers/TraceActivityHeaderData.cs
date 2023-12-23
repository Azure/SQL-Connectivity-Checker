using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TDSClient.TDS.Tranasction.Headers;

namespace TDSClient.TDS.Tranasction.Header.Headers
{
    public class TraceActivityHeaderData : ISqlHeaderData
    {
        public HeaderType HeaderType => HeaderType.TraceActivity;

        public Guid ActivityId { get; set; }
        public uint Size => 16;

        public void Pack(MemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool Unpack(MemoryStream stream)
        {
            throw new NotImplementedException();
        }
    }
}
