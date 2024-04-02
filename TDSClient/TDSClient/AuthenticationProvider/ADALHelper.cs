//  ---------------------------------------------------------------------------
//  <copyright file="ADALHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using TDSClient.TDS.Utilities;

namespace TDSClient.AuthenticationProvider
{
    public class ADALHelper
    {
        private class AdalLoggerCallback : IAdalLogCallback
        {
            public void Log(LogLevel level, string message)
            {
                // Customize how you handle log messages here
                LoggingUtilities.WriteLog($"ADAL Log ({level}): {message}");
            }
        }
        /// <summary>
        /// Gets JWT access token using ADAL with integrated authentication.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static async Task<string> GetSQLAccessTokenFromADALUsingIntegratedAuth(
            string clientId,
            string authority,
            string resource)
        {
            try
            {
                var loggerCallback = new AdalLoggerCallback();
                LoggerCallbackHandler.Callback = loggerCallback;

                UserCredential userCredentials = new UserCredential();
                LoggingUtilities.WriteLog("Acquiring access token...");
                AuthenticationContext authContext = new AuthenticationContext(authority);
                AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientId, userCredentials).ConfigureAwait(false);
                LoggingUtilities.WriteLog("Successfully acquired access token.");
                return result.AccessToken;
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw;
            }
        }

        /// <summary>
        /// Logs possible ADAL exception message.
        /// </summary>
        /// <param name="message">Custom message</param>
        private static void LogException(Exception ex)
        {
            if (ex is AdalException adalException)
            {
                LoggingUtilities.WriteLog($"Client exception occurred when trying to acquire a JWT token: {adalException.Message}");
                LoggingUtilities.WriteLog($"Error code: {adalException.ErrorCode}");
            }
            else if (ex is AdalServiceException serviceException)
            {
                LoggingUtilities.WriteLog($"Service exception occurred when trying to acquire a JWT token: {serviceException.Message}");
                LoggingUtilities.WriteLog($"HTTP status code: {serviceException.StatusCode}");
            }
            else
            {
                LoggingUtilities.WriteLog($"An unexpected error occurred when trying to acquire a JWT token: {ex.Message}");
            }
        }
    }
}
