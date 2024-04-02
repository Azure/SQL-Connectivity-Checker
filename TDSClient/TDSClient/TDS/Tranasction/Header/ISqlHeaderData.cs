using System;
using System.Collections.Generic;
using System.Text;
using TDSClient.TDS.Interfaces;
using TDSClient.TDS.Tranasction.Header;

namespace TDSClient.TDS.Tranasction.Headers
{
    public interface ISqlHeaderData : IPackageable
    {
        HeaderType HeaderType { get; }
        uint Size { get; }
    }
}
