using System.IO;
using TDSClient.TDS.Interfaces;

namespace TDSClient.TDS.Tokens
{
    public abstract class TDSToken : ITDSPacketData
    {
        public abstract ushort Length();
        public abstract void Pack(MemoryStream stream);
        public abstract bool Unpack(MemoryStream stream);
    }
}
