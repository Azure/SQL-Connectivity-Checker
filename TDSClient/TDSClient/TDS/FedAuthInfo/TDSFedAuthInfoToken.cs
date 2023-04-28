﻿using System.IO;
using System.Collections.Generic;
using TDSClient.TDS.Utilities;

using TDSClient.TDS.Tokens;

namespace TDSClient.TDS.FedAuthInfo
{
    /// <summary>
    /// FeatureAck token definition.
    /// </summary>
    public class TDSFedAuthInfoToken : TDSToken
    {
        /// <summary>
        /// Collection of feature extension acknoeldged options
        /// </summary>
        public SortedDictionary<int, TDSFedAuthInfoOption> Options { get; private set; }

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public TDSFedAuthInfoToken()
        {
            // Initialize options collection
            Options = new SortedDictionary<int, TDSFedAuthInfoOption>();
        }

        /// <summary>
        /// Inflating constructor.
        /// </summary>
        /// <param name="source"></param>
        public TDSFedAuthInfoToken(MemoryStream source) :
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
            TDSFedAuthInfoId currentFeatureType = TDSFedAuthInfoId.Invalid;

            uint infoDataLength = 0;
            uint infoDataOffset = 0;

            uint tokenLength = LittleEndianUtilities.ReadUInt(source);
            uint countOfIds = LittleEndianUtilities.ReadUInt(source);

            int i = 0;

            do
            {
                // Read feature type.
                // 
                currentFeatureType = (TDSFedAuthInfoId)source.ReadByte();

                // Ensure we're not looking at the terminator.
                // 
                switch (currentFeatureType)
                {
                    case TDSFedAuthInfoId.STSURL:
                        {
                            // Create an STSURL option.
                            // 
                            infoDataLength = LittleEndianUtilities.ReadUInt(source);
                            infoDataOffset = LittleEndianUtilities.ReadUInt(source);
                            Options.Add(i++, new TDSFedAuthInfoOptionSTSURL(infoDataLength));
                            break;
                        }

                    case TDSFedAuthInfoId.SPN:
                        {
                            // Create SPN option.
                            // 
                            infoDataLength = LittleEndianUtilities.ReadUInt(source);
                            infoDataOffset = LittleEndianUtilities.ReadUInt(source);
                            Options.Add(i++, new TDSFedAuthInfoOptionSPN(infoDataLength));
                            break;
                        }

                    default:
                        {
                            // Create a new generic option
                            // 
                            break;
                        }
                }
            }
            while (--countOfIds > 0);

            foreach (TDSFedAuthInfoOption infoOption in Options.Values)
            {
                infoOption.Unpack(source);
            }

            // We're done inflating
            return true;
        }

        /// <summary>
        /// Deflate the token.
        /// </summary>
        /// <param name="destination">Stream the token to deflate to.</param>
        public override void Pack(MemoryStream destination)
        {
            // Write token identifier
            destination.WriteByte((byte)TDSTokenType.FedAuthInfo);

            // Length of all of the options' FedAuthInfoID, FedAuthInfoDataLen, and FedAuthInfoDataOffset fields.
            // For each option, 1 byte for ID, 4 bytes for DataLen, 4 bytes for offset.
            uint optionsLen = ((uint)Options.Count) * (sizeof(uint) + sizeof (uint) + sizeof(byte));

            // Total length of the token, not including token identifier.
            // 4 bytes for CountOfInfoIDs, plus optionsLen. FedAuthInfoData length is added below.
            uint tokenLength = sizeof(uint) + optionsLen;

            // add to tokenLength the length of each option's data
            MemoryStream[] optionStreams = new MemoryStream[Options.Count];
            for (int i = 0; i < Options.Count; i++)
            {
                optionStreams[i] = new MemoryStream();
                Options[i].Pack(optionStreams[i]);
                tokenLength += (uint)optionStreams[i].Length;
            }

            // Write TokenLength
            LittleEndianUtilities.WriteUInt(destination, tokenLength);

            // Write CountOfInfoIDs
            LittleEndianUtilities.WriteUInt(destination, (uint)Options.Count);

            // Write FedAuthInfoOpt fields.
            // Offset is measured from address of CountOfInfoIDs, so start currOffset
            // pointing after CountOfInfoIDs and FedAuthInfoOpt
            uint currOffset = 4 + optionsLen;
            for (int i = 0; i < Options.Count; i++)
            {
                destination.WriteByte((byte)Options[i].FedAuthInfoId);
                LittleEndianUtilities.WriteUInt(destination, (uint)optionStreams[i].Length);
                LittleEndianUtilities.WriteUInt(destination, currOffset);

                currOffset += (uint)optionStreams[i].Length;
            }

            // Write FedAuthInfoData
            for (int i = 0; i < Options.Count; i++)
            {
                optionStreams[i].WriteTo(destination);
            }
        }

                /// <summary>
        /// TDS Token length
        /// </summary>
        /// <returns>Returns TDS Token length</returns>
        public override ushort Length() 
        {
            return 1;
        }

        public override bool Equals(object obj) 
        {
            return true;
        }
    }
}