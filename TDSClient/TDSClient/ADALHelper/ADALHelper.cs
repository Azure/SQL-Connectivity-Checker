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
        
        /// <summary>
        /// Gets JWT access token using ADAL with username and password.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <param name="clientId"></param>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<string> GetSQLAccessTokenFromADALUsingUsernamePassword(
            string authority,
            string resource,
            string clientId,
            string userId,
            string password)
        {
            UserPasswordCredential userCredentials = new UserPasswordCredential(userId, password);
            return await GetAccessToken(authority, resource, clientId, userCredentials).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets JWT access token using ADAL with integrated authentication.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static async Task<string> GetSQLAccessTokenFromADALUsingIntegratedAuth(
            string authority,
            string resource,
            string clientId)
        {
            UserCredential userCredentials = new UserCredential();
            return await GetAccessToken(authority, resource, clientId, userCredentials).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets AAD access token to Azure SQL using interactive authentication.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        //public static async Task<string> GetSQLAccessTokenFromADALInteractively(string authority, string clientId)
        //{
            // // Validate input parameters
            // if (string.IsNullOrEmpty(authority))
            //     throw new ArgumentException("Authority cannot be null or empty.", nameof(authority));

            // if (string.IsNullOrEmpty(clientId))
            //     throw new ArgumentException("ClientId cannot be null or empty.", nameof(clientId));

            // string[] scopes = new string[] { "https://database.windows.net/.default" };

            // var app = PublicClientApplicationBuilder.Create(clientId)
            //     .WithDefaultRedirectUri()
            //     .Build();

            // AuthenticationResult result;

            // try
            // {
            //     result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();
            //     LoggingUtilities.WriteLog($"  Successfully acquired access token.");

            //     return result.AccessToken;
            // }
            // catch (MsalServiceException ex)
            // {
            //     LoggingUtilities.WriteLog($"Service exception: {ex.Message}");

            //     LoggingUtilities.WriteLog($"Error code: {ex.ErrorCode}");
            //     LoggingUtilities.WriteLog($"HTTP status code: {ex.StatusCode}");

            //     throw;
            // }
            // catch (MsalClientException ex)
            // {
            //     // MSAL client exception occurred
            //     LoggingUtilities.WriteLog($"Client exception: {ex.Message}");

            //     throw;
            // }
            // catch (Exception ex)
            // {
            //     // An unexpected error occurred
            //     LoggingUtilities.WriteLog($"An unexpected error occurred: {ex.Message}");

            //     throw;
            // }
        //}

        /// <summary>
        /// Gets access token using provided credential.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <param name="clientId"></param>
        /// <param name="credential"></param>
        /// <returns></returns>
        private static async Task<string> GetAccessToken(
            string authority,
            string resource,
            string clientId,
            UserCredential credential = null)
        {
            try
            {
                LoggingUtilities.WriteLog("Acquiring access token...");
                AuthenticationContext authContext = new AuthenticationContext(authority);
                AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientId, credential).ConfigureAwait(false);
                LoggingUtilities.WriteLog("Successfully acquired access token.");
                return result.AccessToken;
            }
            catch (AdalServiceException ex)
            {
                LogException("Service exception occurred when trying to acquire a JWT token", ex);
                throw;
            }
            catch (AdalException ex)
            {
                LogException("Client exception occurred when trying to acquire a JWT token", ex);
                throw;
            }
            catch (Exception ex)
            {
                LogException("An unexpected error occurred when trying to acquire a JWT token", ex);
                throw;
            }
        }

        /// <summary>
        /// Logs possible ADAL exception message.
        /// </summary>
        /// <param name="message">Custom message</param>
        /// <param name="ex">Exception</param>
        private static void LogException(string message, Exception ex)
        {
            if (ex is AdalException adalException)
            {
                LoggingUtilities.WriteLog($"{message}: {adalException.Message}");
                LoggingUtilities.WriteLog($"Error code: {adalException.ErrorCode}");
            }

            if (ex is AdalServiceException serviceException)
            {
                LoggingUtilities.WriteLog($"HTTP status code: {serviceException.StatusCode}");
            }
        }
    }
}