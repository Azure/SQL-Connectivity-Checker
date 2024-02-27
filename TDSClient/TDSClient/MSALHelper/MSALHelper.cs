//  ---------------------------------------------------------------------------
//  <copyright file="MSALHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using TDSClient.TDS.Utilities;

namespace TDSClient.MSALHelper
{
    public class MSALHelper
    {
        /// <summary>
        /// Gets AAD access token to Azure SQL using user credentials (username and password).
        /// </summary>
        /// <param name="authoritystring"></param>
        /// <param name="clientId"></param>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<string> GetSQLAccessTokenFromMSALUsingUsernamePassword(string authority, string clientId, string userId, string password)
        {
            var app = CreateClientApp(authority, clientId);

            string[] scopes = new[] { "https://database.windows.net/.default" };

            try
            {
                LoggingUtilities.WriteLog("Attempting to acquire access token using username and password.");
                var result = await app.AcquireTokenByUsernamePassword(scopes, userId, password)
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                LoggingUtilities.WriteLog($"  Successfully acquired access token.");
                return result.AccessToken;
            }
            catch (MsalServiceException ex)
            {
                LoggingUtilities.WriteLog($"Service exception: {ex.Message}");

                LoggingUtilities.WriteLog($"Error code: {ex.ErrorCode}");
                LoggingUtilities.WriteLog($"HTTP status code: {ex.StatusCode}");

                throw;
            }
            catch (MsalClientException ex)
            {
                LoggingUtilities.WriteLog($"Client exception: {ex.Message}");

                throw;
            }
            catch (Exception ex)
            {
                LoggingUtilities.WriteLog($"An unexpected error occurred: {ex.Message}");

                throw;
            }
        }

        /// <summary>
        /// Gets AAD access token to Azure SQL using integrated authentication.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static async Task<string> GetSQLAccessTokenFromMSALUsingIntegratedAuth(string authority, string clientId)
        {
            var app = CreateClientApp(authority, clientId);

            string[] scopes = new[] { "https://database.windows.net/.default" };

            try
            {
                LoggingUtilities.WriteLog("Attempting to acquire access token using integrated auth.");
                var result = await app.AcquireTokenByIntegratedWindowsAuth(scopes)
                    .ExecuteAsync(CancellationToken.None)
                    .ConfigureAwait(false);

                LoggingUtilities.WriteLog($"  Successfully acquired access token.");

                return result.AccessToken;
            }
            catch (MsalServiceException ex)
            {
                LoggingUtilities.WriteLog($"Service exception: {ex.Message}");

                LoggingUtilities.WriteLog($"Error code: {ex.ErrorCode}");
                LoggingUtilities.WriteLog($"HTTP status code: {ex.StatusCode}");

                throw;
            }
            catch (MsalClientException ex)
            {
                // MSAL client exception occurred
                LoggingUtilities.WriteLog($"Client exception: {ex.Message}");

                throw;
            }
            catch (Exception ex)
            {
                // An unexpected error occurred
                LoggingUtilities.WriteLog($"An unexpected error occurred: {ex.Message}");

                throw;
            }
        }

        /// <summary>
        /// Gets AAD access token to Azure SQL using MFA.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static async Task<string> GetSQLAccessTokenFromMSALUsingMFA(string authority, string clientId)
        {
            var app = CreateClientApp(authority, clientId);

            string[] scopes = new[] { "https://database.windows.net/.default" };

            try
            {
                LoggingUtilities.WriteLog("Attempting to acquire access token using MFA.");

                var result = await app.AcquireTokenInteractive(scopes)
                    .ExecuteAsync(CancellationToken.None)
                    .ConfigureAwait(false);

                // if (result.Account?.)
                // {
                //     result = await app.AcquireTokenWithDeviceCode(scopes, deviceCodeResult =>
                //         {
                //             LoggingUtilities.WriteLog(deviceCodeResult.Message);
                //             return Task.FromResult(0);
                //         }).ExecuteAsync();
                // }

                LoggingUtilities.WriteLog($"  Successfully acquired access token.");

                return result.AccessToken;
            }
            catch (MsalServiceException ex)
            {
                LoggingUtilities.WriteLog($"Service exception: {ex.Message}");

                LoggingUtilities.WriteLog($"Error code: {ex.ErrorCode}");
                LoggingUtilities.WriteLog($"HTTP status code: {ex.StatusCode}");

                throw;
            }
            catch (MsalClientException ex)
            {
                // MSAL client exception occurred
                LoggingUtilities.WriteLog($"Client exception: {ex.Message}");

                throw;
            }
            catch (Exception ex)
            {
                // An unexpected error occurred
                LoggingUtilities.WriteLog($"An unexpected error occurred: {ex.Message}");

                throw;
            }
        }

        private static IPublicClientApplication CreateClientApp(string authority, string clientId)
        {
            return PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority(authority)
                .Build();
        }

        /// <summary>
        /// Logs possible MSAL exception message.
        /// </summary>
        /// <param name="message">Custom message</param>
        /// <param name="ex">Exception</param>
        // private static void LogException(string message, Exception ex)
        // {
        //     if (ex is MsalServiceException adalException)
        //     {
        //         LoggingUtilities.WriteLog($"{message}: {adalException.Message}");
        //         LoggingUtilities.WriteLog($"Error code: {adalException.ErrorCode}");
        //     }

        //     if (ex is MsalClientException serviceException)
        //     {
        //         LoggingUtilities.WriteLog($"HTTP status code: {serviceException.StatusCode}");
        //     }
        // }
    }
}