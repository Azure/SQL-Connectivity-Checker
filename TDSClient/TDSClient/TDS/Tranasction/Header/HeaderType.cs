using System;
using System.Collections.Generic;
using System.Text;

namespace TDSClient.TDS.Tranasction.Header
{
    public enum HeaderType : ushort
    {
        QueryNotifications = 0x1,
        TransactionDescriptor = 0x2,
        TraceActivity = 0x3,
    }
}
