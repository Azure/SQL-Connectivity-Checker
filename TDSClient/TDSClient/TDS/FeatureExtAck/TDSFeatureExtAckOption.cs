using System.IO;
using TDSClient.TDS.Interfaces;

namespace TDSClient.TDS.FeatureExtAck
{
    /// <summary>
    /// A single option of the feature extension acknowledgement block
    /// </summary>
    public abstract class TDSFeatureExtAckOption: IPackageable
    {
        /// <summary>
        /// Feature identifier
        /// </summary>
        public virtual TDSFeatureID FeatureID { get; protected set; }

        /// <summary>
        /// Initialization Constructor.
        /// </summary>
        public TDSFeatureExtAckOption()
        {
        }

        /// <summary>
        /// Inflate the token
        /// </summary>
        public abstract bool Unpack(MemoryStream source);

        /// <summary>
        /// Deflate the token.
        /// </summary>
        public abstract void Pack(MemoryStream destination);
     }
}
