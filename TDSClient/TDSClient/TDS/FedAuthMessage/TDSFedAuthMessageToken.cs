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
	using TDSClient.TDS.Tokens;

	/// <summary>
	/// FedAuthToken Message definition.
	/// </summary>
	public class TDSFedAuthToken : TDSToken
	{
		/// <summary>
		/// ADAL Access Token
		/// </summary>
		public string ADALJWT;

		/// <summary>
		/// Federated Authentication Token
		/// </summary>
		protected byte[] m_token;

		/// <summary>
		/// Nonce
		/// </summary>
		protected byte[] m_nonce;

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
		public TDSFedAuthToken(string ADALJWT) :
			this()
		{
			m_token = Encoding.Unicode.GetBytes(ADALJWT);
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
		public override bool Unpack(MemoryStream source)
		{
			// Read length of entire message
			uint totalLengthOfData = LittleEndianUtilities.ReadUInt(source);

			// Read length of the fedauth token
			uint tokenLength = LittleEndianUtilities.ReadUInt(source);

			// Read the fedauth token
			m_token = new byte[tokenLength];
			source.Read(m_token, 0, (int)tokenLength);

			// Read nonce if it exists
			if (totalLengthOfData > tokenLength)
			{
				m_nonce = new byte[totalLengthOfData - tokenLength];
				source.Read(m_nonce, 0, (int)(totalLengthOfData - tokenLength));
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
		public override void Pack(MemoryStream destination)
		{

			// Write the total Length
			uint totalLengthOfData = (uint)(sizeof(uint) + m_token.Length + ((m_nonce != null) ? m_nonce.Length : 0));
			LittleEndianUtilities.WriteUInt(destination, totalLengthOfData);

			// Write the Length of FedAuthToken
			LittleEndianUtilities.WriteUInt(destination, (uint)m_token.Length);

			// Write Access Token
			destination.Write(m_token, 0, m_token.Length);

			// Write Nonce
			if (m_nonce != null)
			{
				destination.Write(m_nonce, 0, m_nonce.Length);
			}
		}

		public override ushort Length() 
        {
            return (ushort)(sizeof(ushort) + (ushort)m_token.Length);
        }

		// override object.Equals
		public override bool Equals(object obj)
		{
			return true;
		}
	
	}
}