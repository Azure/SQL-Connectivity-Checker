//  ---------------------------------------------------------------------------
//  <copyright file="TDSFedAuthInfoToken.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.FedAuthInfo
{
    using System.IO;
    using System.Collections.Generic;
    using System.Collections;

    using TDSClient.TDS.Utilities;
    using TDSClient.TDS.Tokens;
    using System;

#pragma warning disable CS0659
    public class TDSFedAuthInfoToken : TDSToken
    #pragma warning restore CS0659
    {
        /// <summary>
        /// Collection of feature extension acknowledged options
        /// </summary>
        public SortedDictionary<int, TDSFedAuthInfoOption> Options { get; private set; }

        /// <summary>
        /// Length of the Token.
        /// </summary>
        public uint TokenLength { get; private set; }

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public TDSFedAuthInfoToken()
        {
            Options = new SortedDictionary<int, TDSFedAuthInfoOption>();
        }

        /// <summary>
        /// Unpacking constructor.
        /// </summary>
        /// <param name="source"></param>
        public TDSFedAuthInfoToken(MemoryStream source) :
            this()
        {
            Unpack(source);
        }

        /// <summary>
        /// Unpack the token
        /// NOTE: This operation is not continuable and assumes that the entire token is available in the stream
        /// </summary>
        /// <param name="source">Stream to unpack the token from.</param>
        /// <returns>True in case of success, false otherwise.</returns>
        public override bool Unpack(MemoryStream source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // We skip the token identifier because it is read by token factory
            TDSFedAuthInfoId currentFeatureType;

            uint infoDataLength;
            TokenLength = LittleEndianUtilities.ReadUInt(source);
            uint countOfIds = LittleEndianUtilities.ReadUInt(source);

            int i = 0;

            do
            {
                currentFeatureType = (TDSFedAuthInfoId)source.ReadByte();

                switch (currentFeatureType)
                {
                    case TDSFedAuthInfoId.STSURL:
                        {
                            infoDataLength = LittleEndianUtilities.ReadUInt(source);
                            _ = LittleEndianUtilities.ReadUInt(source);
                            Options.Add(i++, new TDSFedAuthInfoOptionSTSURL(infoDataLength));
                            break;
                        }

                    case TDSFedAuthInfoId.SPN:
                        {
                            infoDataLength = LittleEndianUtilities.ReadUInt(source);
                            _ = LittleEndianUtilities.ReadUInt(source);
                            Options.Add(i++, new TDSFedAuthInfoOptionSPN(infoDataLength));
                            break;
                        }

                    default:
                        {
                            break;
                        }
                }
            }
            while (--countOfIds > 0);

            foreach (TDSFedAuthInfoOption infoOption in Options.Values)
            {
                infoOption.Unpack(source);
            }

            return true;
        }

        /// <summary>
        /// Pack the token.
        /// </summary>
        /// <param name="destination">Stream to pack the token to.</param>
        public override void Pack(MemoryStream destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            destination.WriteByte((byte)TDSTokenType.FedAuthInfo);

            // Length of all of the options' FedAuthInfoID, FedAuthInfoDataLen, and FedAuthInfoDataOffset fields.
            // For each option, 1 byte for ID, 4 bytes for DataLen, 4 bytes for offset.
            uint optionsLen = ((uint)Options.Count) * (sizeof(uint) + sizeof (uint) + sizeof(byte));

            // Total length of the token, not including token identifier.
            // 4 bytes for CountOfInfoIDs, plus optionsLen. FedAuthInfoData length is added below.
            uint tokenLength = sizeof(uint) + optionsLen;

            // Add to tokenLength the length of each option's data
            MemoryStream[] optionStreams = new MemoryStream[Options.Count];
            for (int i = 0; i < Options.Count; i++)
            {
                optionStreams[i] = new MemoryStream();
                Options[i].Pack(optionStreams[i]);
                tokenLength += (uint)optionStreams[i].Length;
            }

            LittleEndianUtilities.WriteUInt(destination, tokenLength);

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
        /// Returns Fed Auth Info Token length.
        /// </summary>
        /// <returns>Returns Fed Auth Info Token length</returns>
        public override ushort Length() 
        {
            return (ushort)(TokenLength + sizeof(byte) + sizeof(uint));
        }

        /// <summary>
		/// Compares.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var otherToken = (TDSFedAuthInfoToken)obj;
            return Length() == otherToken.Length()
                && TokenLength == otherToken.TokenLength
                && AreDictionariesEqual(Options, otherToken.Options);
        }

        /// <summary>
        /// Compares two dictionaries.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict1"></param>
        /// <param name="dict2"></param>
        /// <returns></returns>
        private static bool AreDictionariesEqual<TKey, TValue>(SortedDictionary<TKey, TValue> dict1, SortedDictionary<TKey, TValue> dict2)
        {
            // Compare key-value pairs
            return StructuralComparisons.StructuralEqualityComparer.Equals(dict1, dict2);
        }
    }
}