//  ---------------------------------------------------------------------------
//  <copyright file="TDSFedAuthToken.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.FedAuthMessage
{
	using System.IO;
	using System.Text;

	using TDSClient.TDS.Utilities;
	using TDSClient.TDS.Interfaces;

	using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System;
    using TDSClient.TDS.Login7;

    /// <summary>
    /// FedAuthToken Message definition.
    /// </summary>
	#pragma warning disable CS0659
    public class TDSFedAuthToken : ITDSPacketData, IEquatable<TDSFedAuthToken>
	#pragma warning restore CS0659
    {
        /// <summary>
        /// Federated Authentication Token
        /// </summary>
        private byte[] Token;

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
		public TDSFedAuthToken(string ADALJWTToken) :
			this()
		{
			byte[] tokenBytes = Encoding.Unicode.GetBytes(ADALJWTToken);
			Token = new byte[tokenBytes.Length];
			tokenBytes.CopyTo(Token, 0);
		}

		/// <summary>
		/// Inflating constructor.
		/// </summary>
		/// <param name="source"></param>
		public TDSFedAuthToken(MemoryStream source) :
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
		public bool Unpack(MemoryStream source)
		{
			// Read length of entire message
			uint totalLengthOfData = LittleEndianUtilities.ReadUInt(source);

			// Read length of the fedauth token
			uint tokenLength = LittleEndianUtilities.ReadUInt(source);

			// Read the fedauth token
			Token = new byte[tokenLength];
			source.Read(Token, 0, (int)tokenLength);

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
		/// Deflate the token.
		/// </summary>
		/// <param name="destination">Stream the token to deflate to.</param>
		public void Pack(MemoryStream destination)
		{
			// Write the total Length
			uint totalLengthOfData = sizeof(uint) + (uint)(Token.Length);
			
			LittleEndianUtilities.WriteUInt(destination, totalLengthOfData);

			LittleEndianUtilities.WriteUInt(destination, (uint)(Token.Length));

			// Write Access Token
			destination.Write(Token, 0, Token.Length);
		}

		public ushort Length() 
        {
            return (ushort)(sizeof(uint) + sizeof(uint) + (ushort)Token.Length);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as TDSLogin7PacketData);
        }

        public bool Equals(TDSFedAuthToken obj)
		{
			return this.Length() == obj.Length()
					&& this.Token.Length == obj.Token.Length
					&& this.Token.SequenceEqual(obj.Token)
					&& this.Nonce.Length == obj.Nonce.Length
                    && this.Token.SequenceEqual(obj.Token);
		}
	
	}
}