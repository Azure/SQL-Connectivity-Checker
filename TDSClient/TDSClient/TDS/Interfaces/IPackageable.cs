using System.IO;

namespace TDSClient.TDS.Interfaces
{
    public interface IPackageable
    {
        void Pack(MemoryStream stream);
        bool Unpack(MemoryStream stream);
    }
}
