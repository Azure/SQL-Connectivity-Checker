using System.IO;
using TDSClient.TDS.Interfaces;

namespace TDSClient.TDS
{
    /// <summary>
    /// Container for the packet data
    /// </summary>
    public abstract class TDSPacketToken: IPackageable
    {
        /// <summary>
        /// Inflate the token
        /// </summary>
        /// <param name="source">Stream to inflate the token from</param>
        /// <returns>TRUE if inflation is complete</returns>
        public abstract bool Unpack(MemoryStream source);

        /// <summary>
        /// Deflate the token
        /// </summary>
        /// <param name="destination">Stream to deflate token to</param>
        public abstract void Pack(MemoryStream destination);
    }
}