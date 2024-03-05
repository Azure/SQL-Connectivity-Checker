//  ---------------------------------------------------------------------------
//  <copyright file="TDSLogin7GenericOptionToken.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Login7
{
    using System.IO;
    
    using TDSClient.TDS.Utilities;

    /// <summary>
    /// Option token that can contain any data and doesn't have specialized inflation/deflation logic
    /// </summary>
    public class TDSLogin7GenericOptionToken : TDSLogin7FeatureOptionToken
    {
        /// <summary>
        /// Data that the token is carrying
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TDSLogin7GenericOptionToken()
        {
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        public TDSLogin7GenericOptionToken(TDSFeatureID featureID)
        {
            FeatureID = featureID;
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        public TDSLogin7GenericOptionToken(TDSFeatureID featureID, byte[] data) :
            this(featureID)
        {
            Data = data;
        }

        /// <summary>
        /// Inflating constructor
        /// </summary>		
        public TDSLogin7GenericOptionToken(MemoryStream source) :
            this()
        {
            Unpack(source);
        }


        /// <summary>
        /// Inflate the Feature option
        /// </summary>
        /// <param name="source">Stream to inflate the token from</param>
        /// <returns>TRUE if inflation is complete</returns>
        public override bool Unpack(MemoryStream source)
        {
            Size = 0;
            int length = (int)BigEndianUtilities.ReadUInt(source);
            Size += sizeof(int);
            Data = new byte[length];
            source.Read(Data, 0, Data.Length);
            Size += (uint)length;
            return true;
        }

        /// <summary>
        /// Deflate the token
        /// </summary>
        /// <param name="destination">Stream the token to deflate to.</param>
        public override void Pack(MemoryStream destination)
        {
            destination.WriteByte((byte)FeatureID);

            if (Data == null)
            {
                BigEndianUtilities.WriteUInt(destination, 0);
            }
            else
            {
                BigEndianUtilities.WriteUInt(destination, (uint)Data.Length);

                destination.Write(Data, 0, Data.Length);
            }
        }
    }
}