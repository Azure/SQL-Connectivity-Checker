using System.IO;
using TDSClient.TDS.Interfaces;
using TDSClient.TDS.Tranasction.Header;
using TDSClient.TDS.Tranasction.Header.Headers;
using TDSClient.TDS.Utilities;

namespace TDSClient.TDS.Tranasction.Headers
{
    public class SqlHeaderPacketData : IPackageable
    {
        internal SqlHeaderPacketData()
        {

        }

        public SqlHeaderPacketData(ISqlHeaderData data)
        {
            Data = data;
        }

        public ISqlHeaderData Data { get; set; }

        public uint Size => Data.Size + sizeof(ushort) + sizeof(uint);

        public void Pack(MemoryStream stream)
        {
            BigEndianUtilities.WriteUInt(stream, Size);
            BigEndianUtilities.WriteUShort(stream, (ushort)Data.HeaderType);
            Data.Pack(stream);
        }

        public bool Unpack(MemoryStream stream)
        {
            var headerLength = BigEndianUtilities.ReadUInt(stream);
            var headerType = (HeaderType)BigEndianUtilities.ReadUShort(stream);
            ISqlHeaderData data;
            switch(headerType)
            {
                case HeaderType.QueryNotifications:
                    data = new QueryNotificationsHeaderData();
                    break;
                case HeaderType.TransactionDescriptor:
                    data = new TransactionDescriptorHeaderData();
                    break;
                case HeaderType.TraceActivity:
                    data = new TraceActivityHeaderData();
                    break;
                default:
                    stream.Seek(headerLength - (sizeof(ushort) + sizeof(uint)), SeekOrigin.Current);
                    return false;
            }
            if (!data.Unpack(stream))
            {
                return false;
            }
            Data = data;
            return true;
        }
    }
}
