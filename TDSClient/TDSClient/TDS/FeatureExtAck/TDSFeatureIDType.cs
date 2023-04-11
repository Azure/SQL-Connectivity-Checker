namespace TDSClient.TDS.FeatureExtAck
{
    /// <summary>
    /// Type of the Feature ID
    /// </summary>
    public enum TDSFeatureIDType : byte
    {        
        SessionRecovery = 0x01,
        FederatedAuthentication = 0x02,
        Terminator = 0xFF
    }
}