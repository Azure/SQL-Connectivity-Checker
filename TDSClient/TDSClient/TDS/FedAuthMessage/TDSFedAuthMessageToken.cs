using System.IO;
using System.Collections.Generic;
using System.Text;

using TDSClient.TDS;
using TDSClient.TDS.Utilities;

namespace TDSClient.TDS.FedAuthMessage
{
	/// <summary>
	/// FedAuthToken Message definition.
	/// </summary>
	public class TDSFedAuthToken : TDSPacketToken
	{
		/// <summary>
		/// Sample ADAL Token
		/// </summary>
		public const string ADALJWT = @"eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6InIwbW96XzltN3JWTng1OGtTX0JfaW1jdE45SSJ9.eyJpc3MiOiJoYXJpc3VkYW5pc3N1ZXIiLCJhdWQiOiJzcWxkYmF1ZGllbmNlIiwibmJmIjoxMzk5MTAwNDAwLCJleHAiOjE0MDE4NjUyMDAsIm9pZCI6IjhmYmJiYWQ1LTU3MDEtNDdkZC1hNjA3LTUxMGY0ZDI0Mzc5NiIsIm5hbWVpZCI6ImFiNWRlY2Q0LTRlNzQtNDVlOC04YmU4LTI4NDA5YmNkZDFmMUBlOGU2YTJhNS1iNmY1LTQxNzMtYjE2YS1lNDM3ODlkNjEwMWMifQ.SjsAk68hTqtHetXy2RFuqc_SEuX0mWjqMvTf0Sd-9F2XEpP85t_40yxFuD99mJkN3s1RcGN35nh10fYTyQYeHHLF5wmC0DHKRja2dJhQ1wITSXFXDCc1WDvpT7v1DR1EIlv-5l-Eg-eFmvVPPYvISROIObUb_Ci3UiFKrLixgovemrx8xhGkp722XPiPalxPPMask6saWx0e9_sfuMVno0MnEH8GyeDGzBiSmOjTQ9yvfKGPyYFBt2tGUda3BoES2IzMt7Qyc8axMz4xidy88TB1_oTzY8adguX_fxVPHDiFCSxropvjtLVG0KTgpnmQv6WYE4xP6RwaS61RtP9GdQ";

		/// <summary>
		/// Federated Authentication Token
		/// </summary>
		protected byte[] m_token;

		/// <summary>
		/// Nonce
		/// </summary>
		protected byte[] m_nonce;

		/// <summary>
		/// Override of fedauth token length.
		/// </summary>
		protected int m_fedAuthTokenLengthOverride = -1;

		/// <summary>
		/// Zero Length data for the token
		/// </summary>
		protected bool m_fZeroLengthData;


		/// <summary>
		/// Public Getter on Token
		/// </summary>
		public byte[] Token 
		{ 
			get 
			{ 
				return m_token; 
			}
		}

		/// <summary>
		/// Public Getter on Nonce
		/// </summary>
		public byte[] Nonce 
		{ 
			get 
			{ 
				return m_nonce;
			}
		}

		/// <summary>
		/// The nature of FedauthToken to be sent to server, for testing the server
		/// </summary>
		public enum TDSFedAuthTokenTestType
		{
			Normal,
			ZeroLength,
			ZeroLengthAuthToken,
			WrongTdsTokenType,
			Fuzzed,
			JsonWebTokenTests,
		}

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
		public TDSFedAuthToken(byte[] token, byte[] nonce, bool fZeroLengthData, int lengthToOverwrite) :
			this()
		{
			m_fZeroLengthData = fZeroLengthData;

			if (token == null)
			{
				token = Encoding.Unicode.GetBytes(ADALJWT);
			}

			m_token = new byte[token.Length];
			token.CopyTo(m_token, 0);

			if (nonce != null)
			{
				m_nonce = new byte[nonce.Length];
				nonce.CopyTo(m_nonce, 0);
			}

			if (lengthToOverwrite != -1)
			{
				m_fedAuthTokenLengthOverride = lengthToOverwrite;
			}
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
			if (m_fZeroLengthData)
			{
				LittleEndianUtilities.WriteUInt(destination, 0);
				return;
			}

			// Write the total Length
			uint totalLengthOfData = (uint)(sizeof(uint) + m_token.Length + ((m_nonce != null) ? m_nonce.Length : 0));
			LittleEndianUtilities.WriteUInt(destination, m_fedAuthTokenLengthOverride != -1 ? (uint)m_fedAuthTokenLengthOverride : totalLengthOfData);

			// Write the Length of FedAuthToken
			LittleEndianUtilities.WriteUInt(destination, (uint)m_token.Length);

			// Write Fake Token
			destination.Write(m_token, 0, m_token.Length);

			// Write Nonce
			if (m_nonce != null)
			{
				destination.Write(m_nonce, 0, m_nonce.Length);
			}
		}
	}
}