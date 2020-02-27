using System.IO;
using TDSClient.TDS.Interfaces;
using TDSClient.TDS.Utilities;

namespace TDSClient.TDS.Client
{
    public class TDSClientTraceID : IPackageable
    {
        private byte[] TraceID;
        private byte[] ActivityID;
        private ulong ActivitySequence;

        public TDSClientTraceID() {}

        public TDSClientTraceID(byte[] traceID, byte[] activityID, ulong activitySequence)
        {
            TraceID = traceID;
            ActivityID = activityID;
            ActivitySequence = activitySequence;
        }

        public void Pack(MemoryStream stream)
        {
            BigEndianUtilities.WriteByteArray(stream, TraceID);
            BigEndianUtilities.WriteByteArray(stream, ActivityID);
            BigEndianUtilities.WriteULong(stream, ActivitySequence);
        }

        public bool Unpack(MemoryStream stream)
        {
            TraceID = BigEndianUtilities.ReadByteArray(stream, 16);
            ActivityID = BigEndianUtilities.ReadByteArray(stream, 16);
            ActivitySequence = BigEndianUtilities.ReadULong(stream);
            return true;
        }
    }
}
