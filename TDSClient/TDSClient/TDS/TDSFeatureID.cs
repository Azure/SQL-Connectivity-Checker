namespace TDSClient.TDS
{
    /// <summary>
    /// TDS feature identifier
    /// </summary>
    public enum TDSFeatureID : byte
    {
        /// <summary>
        /// Session recovery (connection resiliency)
        /// </summary>
        SessionRecovery = 0x01,

        /// <summary>
        /// Federated authentication
        /// </summary>
        FederatedAuthentication = 0x02,

        /// <summary>
        /// Client telemetry
        /// </summary>
        ClientTelemetry = 0x07,

        /// <summary>
        /// Data Classification
        /// </summary>
        DataClassification = 0x09,

        /// <summary>
        /// UTF-8 encoding support
        /// </summary>
        SupportUTF8 = 0x0A,

		/// <summary
		/// Uri Force Refresh
		/// </summary>
		UriForceRefresh = 0x0C,

		/// <summary>
		/// End of the list
		/// </summary>
		Terminator = 0xFF
    }
}