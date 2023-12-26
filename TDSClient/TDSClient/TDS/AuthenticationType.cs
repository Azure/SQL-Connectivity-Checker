namespace TDSClient.TDS
{
    /// <summary>
    /// Type of authentication used
    /// </summary>
    public enum AuthenticationType
    {
        SQLAuthentication,
        AADPasswordAuthentication,
        AADIntegratedAuthentication,
        AADMFAAuthentication
	}
}