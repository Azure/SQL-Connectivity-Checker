namespace TDSClient.TDS.Login7
{
    using System;
    using System.IO;
    using TDSClient.TDS.Interfaces;
	using TDSClient.TDS.PreLogin;
    using TDSClient.TDS.Utilities;
	using System.Security.Cryptography;

	/// <summary>
	/// The Federated authentication library type.
	/// </summary>
	public enum TDSFedAuthLibraryType : byte
	{
		IDCRL = 0x00,
		SECURITY_TOKEN = 0x01,
		ADAL = 0x02,
		UNSUPPORTED = 0x03,
	}

	/// <summary>
	/// The Federated authentication library type.
	/// </summary>
	public enum TDSFedAuthADALWorkflow : byte
	{
		UserPassword = 0x01,
		Integrated = 0X02,
		EMPTY = 0xff, //present NULL 
	}

	/// <summary>
	/// Feature option token definition.
	/// </summary>
	public class TDSLogin7FedAuthOptionToken : TDSLogin7FeatureOptionToken
	{
		/// <summary>
		/// Nonce's length
		/// </summary>
		public static readonly uint NonceDataLength = 32;

		/// <summary>
		/// Signature's length
		/// </summary>
		public static readonly uint SignatureDataLength = 32;

		/// <summary>
		/// Feature type
		/// </summary>
		public override TDSFeatureID FeatureID { get { return TDSFeatureID.FederatedAuthentication; } }

		/// <summary>
		/// Federated Authentication option length
		/// </summary>
		public uint Length
		{
			get
			{
				return (uint)(sizeof(byte) // Option (library + echo)
					+ (WorkflowType == TDSFedAuthADALWorkflow.EMPTY ? 0 : (sizeof(byte)))
					+ sizeof(uint) // Token length variable
					+ (Token == null ? 0 : Token.Length) // Actual token length
					+ (Nonce == null ? 0 : NonceDataLength) // Nonce Length
					+ (ChannelBingingToken == null ? 0 : ChannelBingingToken.Length) // Channel binding token
					+ (Signature == null ? 0 : SignatureDataLength)); // signature
			}
		}

		/// <summary>
		/// Federated authentication library.
		/// </summary>
		public TDSFedAuthLibraryType Library { get; private set; }

		/// <summary>
		/// FedAuthEcho: The intention of this flag is for the client to echo the server’s FEDAUTHREQUIRED prelogin option.
		/// </summary>
		public TdsPreLoginFedAuthRequiredOption Echo { get; private set; }

		/// <summary>
		/// Whether this protocol is requesting further information from server to perform authentication.
		/// </summary>
		public bool IsRequestingAuthenticationInfo { get; private set; }

		/// <summary>
		/// Federated authentication token generated by the specified federated authentication library.
		/// </summary>
		public byte[] Token { get; private set; }

		/// <summary>
		/// Federated authentication ADAL workflow type
		/// </summary>
		public TDSFedAuthADALWorkflow WorkflowType { get; private set; }

		/// <summary>
		/// The nonce provided by the server during prelogin exchange
		/// </summary>
		public byte[] Nonce { get; private set; }

		/// <summary>
		/// Channel binding token associated with the underlying SSL stream.
		/// </summary>
		public byte[] ChannelBingingToken { get; private set; }

		/// <summary>
		/// The HMAC-SHA-256 [RFC6234] of the server-specified nonce
		/// </summary>
		public byte[] Signature { get; private set; }

		/// <summary>
		/// FedAuth ADAL Workflow Type
		/// </summary>
		public TDSFedAuthADALWorkflow Workflow { get; private set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public TDSLogin7FedAuthOptionToken()
		{
		}

		/// <summary>
		/// Initialization Constructor.
		/// </summary>
		public TDSLogin7FedAuthOptionToken(TdsPreLoginFedAuthRequiredOption echo,
											TDSFedAuthLibraryType libraryType,
											byte[] token,
											byte[] nonce,
											byte[] channelBindingToken,
											bool fIncludeSignature,
											bool fRequestingFurtherInfo,
											TDSFedAuthADALWorkflow workflow)
			: this()
		{
			Echo = echo;
			Library = libraryType;
			Token = token;
			Nonce = nonce;
			WorkflowType = TDSFedAuthADALWorkflow.EMPTY;
			ChannelBingingToken = channelBindingToken;
			IsRequestingAuthenticationInfo = fRequestingFurtherInfo;

			if (libraryType == TDSFedAuthLibraryType.ADAL)
			{
				WorkflowType = workflow;
			}
			else
			{
				WorkflowType = TDSFedAuthADALWorkflow.EMPTY;
			}

			Workflow = WorkflowType;

			if (libraryType != TDSFedAuthLibraryType.SECURITY_TOKEN && fIncludeSignature)
			{
				Signature = new byte[SignatureDataLength];
				Signature = _GenerateRandomBytes(32);
			}
		}

		/// <summary>
		/// Inflating constructor
		/// </summary>		
		public TDSLogin7FedAuthOptionToken(MemoryStream source)
			: this()
		{
			// Inflate feature extension data
			Unpack(source);
		}

		/// <summary>
		/// Inflate the token
		/// </summary>
		/// <param name="source">Stream to inflate the token from</param>
		/// <returns>TRUE if inflation is complete</returns>
		public override bool Unpack(MemoryStream source)
		{
			// Reset inflation size
			InflationSize = 0;

			// We skip option identifier because it was read by construction factory
			// Read the length of the data for the option
			uint optionDataLength = BigEndianUtilities.ReadUInt(source);

			// Update inflation offset
			InflationSize += sizeof(uint);

			// Read one byte for the flags
			byte temp = (byte)source.ReadByte();

			// Update inflation offset
			InflationSize += sizeof(byte);

			// Get the bit and set as a fedauth echo bit
			Echo = (TdsPreLoginFedAuthRequiredOption)(temp & 0x01);

			// Get the remaining 7 bits and set as a library.
			Library = (TDSFedAuthLibraryType)(temp >> 1);

			// When using the ADAL library, a FedAuthToken is never included, nor is its length included
			if (Library != TDSFedAuthLibraryType.ADAL)
			{
				// Length of the FedAuthToken
				uint fedauthTokenLen = BigEndianUtilities.ReadUInt(source);

				// Update inflation offset
				InflationSize += sizeof(uint);

				// Check if the fedauth token is in the login7
				if (fedauthTokenLen > 0)
				{
					// Allocate a container
					Token = new byte[fedauthTokenLen];

					// Read the Fedauth token.
					source.Read(Token, 0, (int)fedauthTokenLen);

					// Update inflation offset
					InflationSize += fedauthTokenLen;
				}
			}
			else
			{
				// Instead the workflow is included
				Workflow = (TDSFedAuthADALWorkflow)source.ReadByte();
			}

			switch (Library)
			{
				case TDSFedAuthLibraryType.IDCRL:
					IsRequestingAuthenticationInfo = false;
					return ReadIDCRLLogin(source, optionDataLength);

				case TDSFedAuthLibraryType.SECURITY_TOKEN:
					IsRequestingAuthenticationInfo = false;
					return ReadSecurityTokenLogin(source, optionDataLength);

				case TDSFedAuthLibraryType.ADAL:
					IsRequestingAuthenticationInfo = true;
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// Deflate the token
		/// </summary>
		/// <param name="destination">Stream to deflate token to</param>
		public override void Pack(MemoryStream destination)
		{
			// Write option identifier
			destination.WriteByte((byte)FeatureID);

			// Calculate Feature Data length
			uint optionDataLength = (uint)(sizeof(byte) // Options size (library and Echo)
									+ (WorkflowType == TDSFedAuthADALWorkflow.EMPTY ? 0 : (sizeof(byte))) //ADAL workflow type
									+ ((Token == null && IsRequestingAuthenticationInfo) ? 0 : sizeof(uint)) // Fedauth token length
									+ (Token == null ? 0 : (uint)Token.Length) // Fedauth Token
									+ (Nonce == null ? 0 : NonceDataLength) // Nonce
									+ (ChannelBingingToken == null ? 0 : (uint)ChannelBingingToken.Length) // Channel binding
									+ (Signature == null ? 0 : SignatureDataLength)); // Signature

			// Write the cache length into the destination
			BigEndianUtilities.WriteUInt(destination, optionDataLength);

			// Construct a byte from fedauthlibrary and fedauth echo.
			byte temp = (byte)((((byte)(Library) << 1) | (byte)(Echo)));
			destination.WriteByte(temp);

			//write ADAL workflow type 
			if (Library == TDSFedAuthLibraryType.ADAL && WorkflowType != TDSFedAuthADALWorkflow.EMPTY)
			{
				destination.WriteByte((byte)WorkflowType);
			}

			// Write FederatedAuthenticationRequired token.
			if (Token == null && !IsRequestingAuthenticationInfo)
			{
				// Write the length of the token is 0
				BigEndianUtilities.WriteUInt(destination, 0);
			}
			else if (Token != null)
			{
				// Write the FederatedAuthenticationRequired token length.
				BigEndianUtilities.WriteUInt(destination, (uint)Token.Length);

				// Write the token.
				destination.Write(Token, 0, Token.Length);
			}

			if (Nonce != null)
			{
				// Write the nonce
				destination.Write(Nonce, 0, Nonce.Length);
			}

			// Write the Channel Binding length
			if (ChannelBingingToken != null)
			{
				destination.Write(ChannelBingingToken, 0, ChannelBingingToken.Length);
			}

			if (Signature != null)
			{
				// Write Signature
				destination.Write(Signature, 0, (int)SignatureDataLength);
			}
		}

		/// <summary>
		/// Read the stream for IDCRL based login
		/// </summary>
		/// <param name="source">source</param>
		/// <param name="optionDataLength">option data length</param>
		/// <returns></returns>
		private bool ReadIDCRLLogin(Stream source, uint optionDataLength)
		{
			// Allocate a container
			Nonce = new byte[NonceDataLength];

			// Read the nonce
			source.Read(Nonce, 0, (int)NonceDataLength);

			// Update inflation offset
			InflationSize += NonceDataLength;

			// Calculate the Channel binding data length.
			uint channelBindingTokenLength = optionDataLength
											- sizeof(byte) // Options size (library and Echo)
											- sizeof(uint) // Token size
											- (Token == null ? 0 : (uint)Token.Length) // Token
											- NonceDataLength // Nonce length
											- SignatureDataLength; // Signature Length

			// Read the channelBindingToken
			if (channelBindingTokenLength > 0)
			{
				// Allocate a container
				ChannelBingingToken = new byte[channelBindingTokenLength];

				// Read the channel binding part.
				source.Read(ChannelBingingToken, 0, (int)channelBindingTokenLength);

				// Update inflation offset
				InflationSize += channelBindingTokenLength;
			}

			// Allocate Signature
			Signature = new byte[SignatureDataLength];

			// Read the Signature
			source.Read(Signature, 0, (int)SignatureDataLength);

			// Update inflation offset
			InflationSize += SignatureDataLength;

			return true;
		}

		/// <summary>
		/// Read the stream for SecurityToken based login
		/// </summary>
		/// <param name="source">source</param>
		/// <param name="optionDataLength">option data length</param>
		/// <returns></returns>
		private bool ReadSecurityTokenLogin(Stream source, uint optionDataLength)
		{
			// Check if any data is available
			if (optionDataLength > InflationSize)
			{
				// Allocate a container
				Nonce = new byte[NonceDataLength];

				// Read the nonce
				source.Read(Nonce, 0, (int)NonceDataLength);

				// Update inflation offset
				InflationSize += NonceDataLength;
			}

			return true;
		}

		/// <summary>
		/// Generates random bytes
		/// </summary>
		/// <param name="count">The number of bytes to be generated.</param>
		/// <returns>Generated random bytes.</returns>
		private byte[] _GenerateRandomBytes(int count)
		{
			byte[] randomBytes = new byte[count];

			RNGCryptoServiceProvider gen = new RNGCryptoServiceProvider();
			// Generate bytes
			gen.GetBytes(randomBytes);

			return randomBytes;
		}
	}
}