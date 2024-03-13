//  ---------------------------------------------------------------------------
//  <copyright file="TDSLogin7FedAuthOptionToken.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Login7
{
    using System.IO;
	using System.Security.Cryptography;

	using TDSClient.TDS.PreLogin;
    using TDSClient.TDS.Utilities;
	
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
		EMPTY = 0xff, // NULL 
	}

    /// <summary>
    /// Feature option token definition.
    /// </summary>
    public class TDSLogin7FedAuthOptionToken : TDSLogin7FeatureOptionToken
    {
        private const uint NonceDataLength = 32;
        private const uint SignatureDataLength = 32;

        public override TDSFeatureID FeatureID => TDSFeatureID.FederatedAuthentication;

        public uint Length => 6 * sizeof(byte) + sizeof(byte);

        public TDSFedAuthLibraryType Library { get; private set; }

        public TdsPreLoginFedAuthRequiredOption Echo { get; private set; }

        public bool IsRequestingAuthenticationInfo { get; private set; }

        public byte[] Token { get; private set; }

        public TDSFedAuthADALWorkflow WorkflowType { get; private set; }

        public byte[] Nonce { get; private set; }

        public byte[] ChannelBingingToken { get; private set; }

        public byte[] Signature { get; private set; }

        public TDSFedAuthADALWorkflow Workflow { get; private set; }

        public TDSLogin7FedAuthOptionToken()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="echo"></param>
        /// <param name="libraryType"></param>
        /// <param name="token"></param>
        /// <param name="nonce"></param>
        /// <param name="channelBindingToken"></param>
        /// <param name="fIncludeSignature"></param>
        /// <param name="fRequestingFurtherInfo"></param>
        /// <param name="workflow"></param>
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
            WorkflowType = libraryType == TDSFedAuthLibraryType.ADAL ? workflow : TDSFedAuthADALWorkflow.EMPTY;
            ChannelBingingToken = channelBindingToken;
            IsRequestingAuthenticationInfo = fRequestingFurtherInfo;

            if (libraryType != TDSFedAuthLibraryType.SECURITY_TOKEN && fIncludeSignature)
            {
                Signature = GenerateRandomBytes((int)SignatureDataLength);
            }
        }

        public TDSLogin7FedAuthOptionToken(MemoryStream source)
            : this()
        {
            Unpack(source);
        }

        /// <summary>
        /// Unpack the data from the stream, when receiving this token.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override bool Unpack(MemoryStream source)
        {
            Size = 0;
            uint optionDataLength = BigEndianUtilities.ReadUInt(source);
            Size += sizeof(uint);
            byte temp = (byte)source.ReadByte();
            Size += sizeof(byte);

            // Echo is the last bit of the byte read
            Echo = (TdsPreLoginFedAuthRequiredOption)(temp & 0x01);
            // Library is the first 7 bits of the byte read
            Library = (TDSFedAuthLibraryType)(temp >> 1);

            if (Library != TDSFedAuthLibraryType.ADAL)
            {
                uint fedauthTokenLen = BigEndianUtilities.ReadUInt(source);
                Size += sizeof(uint);

                if (fedauthTokenLen > 0)
                {
                    Token = new byte[fedauthTokenLen];
                    source.Read(Token, 0, (int)fedauthTokenLen);
                    Size += fedauthTokenLen;
                }
            }
            else
            {
                Workflow = (TDSFedAuthADALWorkflow)source.ReadByte();
            }

            switch (Library)
            {
                case TDSFedAuthLibraryType.SECURITY_TOKEN:
                    return ReadSecurityTokenLogin(source, optionDataLength);

                case TDSFedAuthLibraryType.ADAL:
                    IsRequestingAuthenticationInfo = true;
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Pack the token.
        /// </summary>
        /// <param name="destination"></param>
        public override void Pack(MemoryStream destination)
        {
            destination.WriteByte((byte)FeatureID);

            uint optionDataLength = (uint)(sizeof(byte) +
                                    (WorkflowType == TDSFedAuthADALWorkflow.EMPTY ? 0 : sizeof(byte)) +
                                    ((Token == null && IsRequestingAuthenticationInfo) ? 0 : sizeof(uint)) +
                                    (Token == null ? 0 : (uint)Token.Length) +
                                    (Nonce == null ? 0 : NonceDataLength) +
                                    (ChannelBingingToken == null ? 0 : (uint)ChannelBingingToken.Length) +
                                    (Signature == null ? 0 : SignatureDataLength));

            LittleEndianUtilities.WriteUInt(destination, optionDataLength);

            byte temp = (byte)(((byte)Library << 1) | (byte)Echo);
            destination.WriteByte(temp);

            if (Library == TDSFedAuthLibraryType.ADAL && WorkflowType != TDSFedAuthADALWorkflow.EMPTY)
            {
                destination.WriteByte((byte)WorkflowType);
            }

            if (Token == null && !IsRequestingAuthenticationInfo)
            {
                BigEndianUtilities.WriteUInt(destination, 0);
            }
            else if (Token != null)
            {
                BigEndianUtilities.WriteUInt(destination, (uint)Token.Length);

                destination.Write(Token, 0, Token.Length);
            }

            if (Nonce != null)
            {
                destination.Write(Nonce, 0, Nonce.Length);
            }

            if (ChannelBingingToken != null)
            {
                destination.Write(ChannelBingingToken, 0, ChannelBingingToken.Length);
            }

            if (Signature != null)
            {
                destination.Write(Signature, 0, (int)SignatureDataLength);
            }
        }

        /// <summary>
        /// Read the security token login.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="optionDataLength"></param>
        /// <returns></returns>
        private bool ReadSecurityTokenLogin(Stream source, uint optionDataLength)
        {
            if (optionDataLength > Size)
            {
                Nonce = new byte[NonceDataLength];
                source.Read(Nonce, 0, (int)NonceDataLength);
                Size += NonceDataLength;
            }

            return true;
        }

        /// <summary>
        /// Generate random bytes.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private byte[] GenerateRandomBytes(int count)
        {
            byte[] randomBytes = new byte[count];

            using (RNGCryptoServiceProvider gen = new RNGCryptoServiceProvider())
            {
                gen.GetBytes(randomBytes);
            }

            return randomBytes;
        }
    }
}
