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
    public class TDSFedAuthToken : ITDSPacketData
	#pragma warning restore CS0659
    {
        /// <summary>
        /// Federated Authentication Token
        /// </summary>
        private byte[] FedAuthToken {get; set; }

		/// <summary>
		/// Nonce
		/// </summary>
		private byte[] Nonce {get;  set; }

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
			if (JWTAccessToken == null)
			{
				throw new ArgumentNullException(nameof(JWTAccessToken));
			}

			FedAuthToken = Encoding.Unicode.GetBytes(JWTAccessToken);
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
			// Read length of entire message
			uint totalLengthOfData = LittleEndianUtilities.ReadUInt(source);

			// Read length of the fedauth token
			uint tokenLength = LittleEndianUtilities.ReadUInt(source);

			// Read the fedauth token
			FedAuthToken = new byte[tokenLength];
			source.Read(FedAuthToken, 0, (int)tokenLength);

			// Read nonce if it exists
			if (totalLengthOfData > tokenLength)
			{
				Nonce = new byte[totalLengthOfData - tokenLength];
				source.Read(Nonce, 0, (int)(totalLengthOfData - tokenLength));
			}
			else if (tokenLength > totalLengthOfData)
			{
				// token length cannot be greater than the total length of the message
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
			uint totalLengthOfData = sizeof(uint) + (uint)FedAuthToken.Length + (Nonce != null ? (uint)Nonce.Length : 0);
			
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
            return (ushort)(sizeof(uint) + sizeof(uint) + (uint)FedAuthToken.Length);
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

			var otherToken = (TDSFedAuthToken)obj;
			return Length() == otherToken.Length()
				&& FedAuthToken.SequenceEqual(otherToken.FedAuthToken)
				&& Nonce.SequenceEqual(otherToken.Nonce);
        }
	}
}