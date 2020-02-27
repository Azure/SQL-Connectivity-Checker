using TDSClient.TDS.Interfaces;

namespace TDSClient.TDS.Interfaces
{
    public interface ITDSPacketData : IPackageable
    {
        ushort Length();
    }
}
