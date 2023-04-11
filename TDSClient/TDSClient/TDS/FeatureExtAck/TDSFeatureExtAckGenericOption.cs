using System.IO;
using TDSClient.TDS.Utilities;

namespace TDSClient.TDS.FeatureExtAck
{
    /// <summary>
    /// A single option of the feature extension acknowledgement block
    /// </summary>
    public class TDSFeatureExtAckGenericOption : TDSFeatureExtAckOption
    {
        /// <summary>
        /// FeatureAck data length.
        /// </summary>
        public uint FeatureAckDataLen { get; set; }

        /// <summary>
        /// FeatureAck Data.
        /// </summary>
        public byte[] FeatureAckData { get; set; }

        /// <summary>
        /// Initialization Constructor.
        /// </summary>
        public TDSFeatureExtAckGenericOption()
        {
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="type">type of the FeatureExtAckToken.</param>
        public TDSFeatureExtAckGenericOption(TDSFeatureID type)
        {
            FeatureID = type;
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="type">type of the FeatureExtAckToken.</param>
        /// <param name="featureAckDataLen">Length of the data.</param>
        public TDSFeatureExtAckGenericOption(TDSFeatureID type, uint featureAckDataLen)
            : this(type)
        {
            FeatureAckDataLen = featureAckDataLen;
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="type">type of the FeatureExtAckToken.</param>
        /// <param name="featureAckDataLen">Length of the data.</param>
        /// <param name="data">Data of the FeatureAck token.</param>
        public TDSFeatureExtAckGenericOption(TDSFeatureID type, uint featureAckDataLen, byte[] data)
            : this(type, featureAckDataLen)
        {
            FeatureAckData = data;
        }

        /// <summary>
        /// Inflating constructor.
        /// </summary>
        /// <param name="source"></param>
        public TDSFeatureExtAckGenericOption(MemoryStream source)
        {
            Unpack(source);
        }

        /// <summary>
        /// Inflating constructor
        /// </summary>
        /// <param name="source"></param>
        public TDSFeatureExtAckGenericOption(TDSFeatureID featureID, MemoryStream source) :
            this(source)
        {
            FeatureID = featureID;
        }

        /// <summary>
        /// Inflate the token
        /// NOTE: This operation is not continuable and assumes that the entire token is available in the stream
        /// </summary>
        /// <param name="source">Stream to inflate the token from.</param>
        /// <returns>True in case of success, false otherwise.</returns>
        public override bool Unpack(MemoryStream source)
        {
            // We skip the token identifier because it is read by token factory

            // Read the data length.
            FeatureAckDataLen = LittleEndianUtilities.ReadUInt(source);

            // Read the Data.
            if (FeatureAckDataLen > 0)
            {
                FeatureAckData = new byte[FeatureAckDataLen];
                source.Read(FeatureAckData, 0, (int)FeatureAckDataLen);
            }

            return true;
        }

        /// <summary>
        /// Delate the token.
        /// </summary>
        /// <param name="destination">Stream the token to deflate to.</param>
        public override void Pack(MemoryStream destination)
        {
            // Write FeatureID
            destination.WriteByte((byte)FeatureID);

            // Write FeatureAckDataLen
            LittleEndianUtilities.WriteUInt(destination, FeatureAckDataLen);

            // Write data.
            destination.Write(FeatureAckData, 0, (int)FeatureAckDataLen);
        }
    }
}
