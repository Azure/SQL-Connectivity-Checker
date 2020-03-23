//  ---------------------------------------------------------------------------
//  <copyright file="TDSTokenType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Tokens
{
    /// <summary>
    /// Enum describing TDS Token Type
    /// </summary>
    public enum TDSTokenType : byte
    {
        /// <summary>
        /// TDS Alt Metadata Token.
        /// </summary>
        AltMetadata = 0x88,

        /// <summary>
        /// TDS Alt Row Token.
        /// </summary>
        AltRow = 0xd3,

        /// <summary>
        /// TDS Col Info Token.
        /// </summary>
        ColInfo = 0xa5,

        /// <summary>
        /// TDS Col Metadata Token.
        /// </summary>
        ColMetadata = 0x81,

        /// <summary>
        /// TDS Data Classification Token.
        /// </summary>
        DataClassification = 0xa3,

        /// <summary>
        /// TDS Done Token.
        /// </summary>
        Done = 0xfd,

        /// <summary>
        /// TDS Done In Proc Token.
        /// </summary>
        DoneInProc = 0xff,

        /// <summary>
        /// TDS Done Proc Token.
        /// </summary>
        DoneProc = 0xfe,

        /// <summary>
        /// TDS EnvChange Token.
        /// </summary>
        EnvChange = 0xe3,

        /// <summary>
        /// TDS Error Token.
        /// </summary>
        Error = 0xaa,

        /// <summary>
        /// TDS Feature Ext Ack Token.
        /// </summary>
        FeatureExtAck = 0xae,

        /// <summary>
        /// TDS Fed Auth Info Token.
        /// </summary>
        FedAuthInfo = 0xee,

        /// <summary>
        /// TDS Info Token.
        /// </summary>
        Info = 0xab,

        /// <summary>
        /// TDS Login Ack Token.
        /// </summary>
        LoginAck = 0xad,

        /// <summary>
        /// TDS NBC Row Token.
        /// </summary>
        NbcRow = 0xd2,

        /// <summary>
        /// TDS Offset Token.
        /// </summary>
        Offset = 0x78,

        /// <summary>
        /// TDS Order Token.
        /// </summary>
        Order = 0xa9,

        /// <summary>
        /// TDS Return Status Token.
        /// </summary>
        ReturnStatus = 0x79,

        /// <summary>
        /// TDS Return Value Token.
        /// </summary>
        ReturnValue = 0xac,

        /// <summary>
        /// TDS Row Token.
        /// </summary>
        Row = 0xd1,

        /// <summary>
        /// TDS Session State Token.
        /// </summary>
        SessionState = 0xe4,

        /// <summary>
        /// TDS SSPI Token.
        /// </summary>
        SSPI = 0xed,

        /// <summary>
        /// TDS TVP Row Token.
        /// </summary>
        TvpRow = 0x01
    }
}
