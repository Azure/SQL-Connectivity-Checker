//  ---------------------------------------------------------------------------
//  <copyright file="TDSFedAuthToken.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.FedAuthMessage
{
	using System.IO;
	using System.Text;
	using System.Linq;
    using System;

	using TDSClient.TDS.Utilities;
	using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Login7;

    /// <summary>
    /// FedAuthToken Message definition.
    /// </summary>
	#pragma warning disable CS0659
    public class TDSFedAuthToken : ITDSPacketData, IEquatable<TDSFedAuthToken>
	#pragma warning restore CS0659
    {
		/// <summary>
		/// Total length of data, without DataLen itself;
		/// </summary>
		private uint DataLen;

        /// <summary>
        /// Federated Authentication Token
        /// </summary>
        private byte[] FedAuthToken;

		/// <summary>
		/// Nonce
		/// </summary>
		private byte[] Nonce;

		/// <summary>
		/// Default Constructor.
		/// </summary>
		public TDSFedAuthToken()
		{
		}

		/// <summary>
		/// Initialization constructor.
		/// </summary>
		/// <param name="token">Token</param>
		public TDSFedAuthToken(string JWTAccessToken) :
			this()
		{
			byte[] tokenBytes = Encoding.Unicode.GetBytes(JWTAccessToken);
			FedAuthToken = new byte[tokenBytes.Length];
			tokenBytes.CopyTo(FedAuthToken, 0);
		}

		/// <summary>
		/// Unpacking constructor.
		/// </summary>
		/// <param name="source"></param>
		public TDSFedAuthToken(MemoryStream source) :
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
		public bool Unpack(MemoryStream source)
		{
			DataLen = LittleEndianUtilities.ReadUInt(source);

			uint tokenLength = LittleEndianUtilities.ReadUInt(source);

			FedAuthToken = new byte[tokenLength];
			source.Read(FedAuthToken, 0, (int)tokenLength);

			if (DataLen > tokenLength)
			{
				Nonce = new byte[DataLen - tokenLength];
				source.Read(Nonce, 0, (int)(DataLen - tokenLength));
			}
			else if (tokenLength > DataLen)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Pack the token.
		/// </summary>
		/// <param name="destination">Stream to pack the token to.</param>
		public void Pack(MemoryStream destination)
		{
			uint totalLengthOfData = sizeof(uint) + (uint)FedAuthToken.Length + (uint)Nonce.Length;
			
			LittleEndianUtilities.WriteUInt(destination, totalLengthOfData);

			LittleEndianUtilities.WriteUInt(destination, (uint)FedAuthToken.Length);

			destination.Write(FedAuthToken, 0, FedAuthToken.Length);

			if (Nonce!=null)
			{
				destination.Write(Nonce, 0, Nonce.Length);
			}
		}

		/// <summary>
		/// Length of the Fed Auth Token message.
		/// </summary>
		/// <returns></returns>
		public ushort Length() 
        {
            return (ushort)(sizeof(uint) + DataLen);
        }

		/// <summary>
		/// Compares.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as TDSFedAuthToken);
        }

		/// <summary>
		/// Compares.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
        public bool Equals(TDSFedAuthToken obj)
		{
			return Length() == obj.Length()
					&& FedAuthToken.Length == obj.FedAuthToken.Length
					&& FedAuthToken.SequenceEqual(obj.FedAuthToken)
					&& Nonce.Length == obj.Nonce.Length
                    && Nonce.SequenceEqual(obj.Nonce);
		}
	}
}