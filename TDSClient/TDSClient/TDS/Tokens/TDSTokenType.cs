//  ---------------------------------------------------------------------------
//  <copyright file="TDSTokenType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public enum TDSTokenType : byte
    {
        AltMetadata = 0x88,
        AltRow = 0xd3,
        ColInfo = 0xa5,
        ColMetadata = 0x81,
        DataClassification = 0xa3,
        Done = 0xfd,
        DoneInProc = 0xff,
        DoneProc = 0xfe,
        EnvChange = 0xe3,
        Error = 0xaa,
        FeatureExtAck = 0xae,
        FedAuthInfo = 0xee,
        Info = 0xab,
        LoginAck = 0xad,
        NbcRow = 0xd2,
        Offset = 0x78,
        Order = 0xa9,
        ReturnStatus = 0x79,
        ReturnValue = 0xac,
        Row = 0xd1,
        SessionState = 0xe4,
        SSPI = 0xed,
        TvpRow = 0x01
    }
}
