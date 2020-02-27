namespace TDSClient.TDS.Header
{
    public enum TDSMessageStatus : byte
    {
        Normal,
        EndOfMessage,
        IgnoreEvent,
        ResetConnection = 0x08,
        ResetConnectionSkipTran = 0x10
    }
}
