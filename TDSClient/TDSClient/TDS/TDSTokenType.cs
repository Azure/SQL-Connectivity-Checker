namespace TDSClient.TDS
{
    /// <summary>
    /// Types of the tokens in data buffer of the packet
    /// </summary>
    public enum TDSTokenType
    {
        AlternativeMetadata = 088,
        AlternativeRow = 0xD3,
        ColumnInfo = 0xA5,
        ColumnMetadata = 0x81,
        DataClassification = 0xA3,
        Done = 0xFD,
        DoneProcedure = 0xFE,
        DoneInProc = 0xFF,
        EnvironmentChange = 0xE3,
        Error = 0xAA,
        FeatureExtAck = 0xAE,
        FedAuthInfo = 0xEE,
        Info = 0xAB,
        LoginAcknowledgement = 0xAD,
        NBCRow = 0xD2,
        Offset = 0x78,
        Order = 0xA9,
        ReturnStatus = 0x79,
        ReturnValue = 0xAC,
        Row = 0xD1,
        SessionState = 0xE4,
        SSPI = 0xED,
        TableName = 0xA4,
	}
}