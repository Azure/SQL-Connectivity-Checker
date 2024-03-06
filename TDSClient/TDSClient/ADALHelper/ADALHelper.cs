//  ---------------------------------------------------------------------------
//  <copyright file="ADALHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using TDSClient.TDS.Utilities;

namespace TDSClient.ADALHelper
{
    public class ADALHelper
    {
        private static readonly string AdoClientId = "4d079b4c-cab7-4b7c-a115-8fd51b6f8239";

        /// NOTE: not possible to use username password with ADAL in netstandard 2.0 (.net framework possible)
        /// <summary>
        /// Gets JWT access token using ADAL with username and password.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <param name="clientId"></param>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        // public static async Task<string> GetSQLAccessTokenFromADALUsingUsernamePassword(
        //     string authority,
        //     string resource,
        //     string clientId,
        //     string userId,
        //     string password)
        // {
        //     UserPasswordCredential userCredentials = new UserPasswordCredential(userId, password);
        //     return await GetAccessToken(authority, resource, clientId, userCredentials).ConfigureAwait(false);
        // }

        /// <summary>
        /// Gets JWT access token using ADAL with integrated authentication.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static async Task<string> GetSQLAccessTokenFromADALUsingIntegratedAuth(
            string authority,
            string resource)
        {
            UserCredential userCredentials = new UserCredential();
            return await GetAccessToken(authority, resource, userCredentials).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets access token using provided credential.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <param name="credential"></param>
        /// <returns></returns>
        private static async Task<string> GetAccessToken(
            string authority,
            string resource,
            UserCredential credential = null)
        {
            try
            {
                LoggingUtilities.WriteLog("Acquiring access token...");
                AuthenticationContext authContext = new AuthenticationContext(authority);
                AuthenticationResult result = await authContext.AcquireTokenAsync(resource, AdoClientId, credential).ConfigureAwait(false);
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