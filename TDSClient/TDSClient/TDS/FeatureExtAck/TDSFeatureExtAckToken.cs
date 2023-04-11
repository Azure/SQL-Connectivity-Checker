using System.IO;
using System.Collections.Generic;

namespace TDSClient.TDS.FeatureExtAck
{
    /// <summary>
    /// FeatureAck token definition.
    /// </summary>
    public class TDSFeatureExtAckToken : TDSPacketToken
    {
        /// <summary>
        /// Collection of feature extension acknoeldged options
        /// </summary>
        public IList<TDSFeatureExtAckOption> Options { get; set; }

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public TDSFeatureExtAckToken()
        {
            // Initialize options collection
            Options = new List<TDSFeatureExtAckOption>();
        }

        /// <summary>
        /// Initialization constructor.
        /// </summary>
        /// <param name="source"></param>
        public TDSFeatureExtAckToken(params TDSFeatureExtAckOption[] options) :
            this()
        {
            ((List<TDSFeatureExtAckOption>)Options).AddRange(options);
        }

        /// <summary>
        /// Inflating constructor.
        /// </summary>
        /// <param name="source"></param>
        public TDSFeatureExtAckToken(MemoryStream source):
            this()
        {
            Unpack(source);
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
            TDSFeatureID currentFeatureType = TDSFeatureID.FederatedAuthentication;

            do
            {
                // Read feature type
                currentFeatureType = (TDSFeatureID)source.ReadByte();

                // Ensure we're not looking at the terminator
                switch (currentFeatureType)
                {
                    // case TDSFeatureID.SessionRecovery:
                    //     {
                    //         // Create a new option
                    //         Options.Add(new TDSFeatureExtAckSessionStateOption(source));
                    //         break;
                    //     }

                    case TDSFeatureID.FederatedAuthentication:
                        {
                            Options.Add(new TDSFeatureExtAckFederatedAuthenticationOption(source));
                            break;
                        }

                    // case TDSFeatureID.DataClassification:
                    //     {
                    //         // Create a new option
                    //         Options.Add(new TDSFeatureExtAckDataClassificationOption(source));
                    //         break;
                    //     }

                    // case TDSFeatureID.SupportUTF8:
                    //     {
                    //         Options.Add(new TDSFeatureExtAckSupportUTF8Option(source));
                    //         break;
                    //     }

                    case TDSFeatureID.Terminator:
                        {
                            // Do nothing
                            break;
                        }
                    default:
                        {
                            // Create a new generic option
                            Options.Add(new TDSFeatureExtAckGenericOption(currentFeatureType, source));
                            break;
                        }
                }
            }
            while (currentFeatureType != TDSFeatureID.Terminator);

            // We're done inflating
            return true;
        }

        /// <summary>
        /// Deflate the token.
        /// </summary>
        /// <param name="destination">Stream the token to deflate to.</param>
        public override void Pack(MemoryStream destination)
        {
            // Write the token type.
            destination.WriteByte((byte)TDSTokenType.FeatureExtAck);

            // Iterate through all options
            foreach (TDSFeatureExtAckOption option in Options)
            {
                // Deflate the option itself
                option.Pack(destination);
            }

            // Write terminator
            destination.WriteByte((byte)TDSFeatureID.Terminator);
        }
    }
}