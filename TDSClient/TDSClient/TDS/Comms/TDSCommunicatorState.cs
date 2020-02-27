namespace TDSClient.TDS.Comms
{
    public enum TDSCommunicatorState
    {
        Initial,
        SentInitialPreLogin,
        SentLogin7RecordWithCompleteAuthToken,
        LoggedIn
    }
}
