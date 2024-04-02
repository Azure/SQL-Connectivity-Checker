using System.IO;
using TDSClient.TDS.Tranasction.Headers;
using TDSClient.TDS.Utilities;

namespace TDSClient.TDS.Tranasction.Header.Headers
{
    public class QueryNotificationsHeaderData : ISqlHeaderData
    {
        public HeaderType HeaderType => HeaderType.QueryNotifications;
        public uint Size => sizeof(ushort) + (uint)NotifyId.Length * 2 + sizeof(ushort) + (uint)SSBDeployment.Length * 2 + sizeof(ulong);
        public string NotifyId { get; set; }
        public string SSBDeployment { get; set; }
        public ulong NotifyTimeout { get; set; }

        public void Pack(MemoryStream stream)
        {
            BigEndianUtilities.WriteUShort(stream, (ushort)NotifyId.Length);
            BigEndianUtilities.WriteUnicodeStream(stream, NotifyId);
            BigEndianUtilities.WriteUShort(stream, (ushort)SSBDeployment.Length);
            BigEndianUtilities.WriteUnicodeStream(stream, SSBDeployment);
            BigEndianUtilities.WriteULong(stream, NotifyTimeout);
        }

        public bool Unpack(MemoryStream stream)
        {
            var notifyIdLen = BigEndianUtilities.ReadUShort(stream);
            var notifyIdChars = BigEndianUtilities.ReadUnicodeStreamLE(stream, notifyIdLen);
            NotifyId = new string(notifyIdChars);

            var ssbDeploymentLen = BigEndianUtilities.ReadUShort(stream);
            var ssbDeploymentChars = BigEndianUtilities.ReadUnicodeStreamLE(stream, ssbDeploymentLen);
            SSBDeployment = new string(ssbDeploymentChars);

            NotifyTimeout = BigEndianUtilities.ReadULong(stream);
            return true;
        }
    }
}
