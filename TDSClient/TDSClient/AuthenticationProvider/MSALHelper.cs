//  ---------------------------------------------------------------------------
//  <copyright file="MSALHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Identity.Client;
using Microsoft.Identity.Client.AppConfig;

using TDSClient.TDS.Utilities;

namespace TDSClient.AuthenticationProvider
{
    public class MSALHelper
    {
        private static readonly string AdoClientId = "2fd908ad-0664-4344-b9be-cd3e8b574c38";

        /// <summary>
        /// Gets AAD access token to Azure SQL using user credentials (username and password).
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<string> GetSQLAccessTokenFromMSALUsingUsernamePassword(string authority, string resource, string userId, string password)
        {
            ValidateInputParameters(authority, userId, password);

            var app = CreateClientApp(authority);
            string[] scopes = new[] { resource + "/.default" };

            LoggingUtilities.WriteLog("Attempting to acquire access token using username and password.");

            try
            {
                var result = await app.AcquireTokenByUsernamePassword(scopes, userId, password)
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                LoggingUtilities.WriteLog("Successfully acquired access token.");
                return result.AccessToken;
            }
            catch (MsalServiceException ex)
            {
                HandleException(ex);
                throw;
            }
        }

        /// <summary>
        /// Gets AAD access token to Azure SQL using integrated authentication.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task<string> GetSQLAccessTokenFromMSALUsingIntegratedAuth(string authority, string resource, string userId)
        {
            ValidateInputParameters(authority);

            LoggingUtilities.WriteLog("Attempting to acquire access token using integrated auth.");

            var app = CreateClientApp(authority);

            string[] scopes = new[] { resource + "/.default" };

            try
            {
                var result = userId == null ?
                    await app.AcquireTokenByIntegratedWindowsAuth(scopes)
                        .ExecuteAsync(CancellationToken.None)
                        .ConfigureAwait(false) :
                    await app.AcquireTokenByIntegratedWindowsAuth(scopes)
                        .WithUsername(userId)
                        .ExecuteAsync(CancellationToken.None)
                        .ConfigureAwait(false);

                LoggingUtilities.WriteLog("Successfully acquired access token.");
                return result.AccessToken;
            }
            catch (MsalServiceException ex)
            {
                HandleException(ex);
                throw;
            }
        }

        /// <summary>
        /// Gets AAD access token to Azure SQL using interactive authentication.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static async Task<string> GetSQLAccessTokenFromMSALInteractively(string resource, string authority)
        {
            ValidateInputParameters(resource);

            string[] scopes = new string[] { resource + "/.default" };

            LoggingUtilities.WriteLog("Attempting to acquire access token using interactive auth.");
            var app = PublicClientApplicationBuilder.Create(AdoClientId)
                .WithAuthority(authority)
                .WithRedirectUri("http://localhost")
                .WithLogging(LogCallback, LogLevel.Verbose, true)
                .Build();

            try
            {
                var result = await app.AcquireTokenInteractive(scopes)
                    .ExecuteAsync();
                LoggingUtilities.WriteLog($"  Successfully acquired access token.");

                return result.AccessToken;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                throw;
            }
        }

        /// <summary>
        /// Gets AAD access token to Azure SQL using system assigned managed identity.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static async Task<string> GetSQLAccessTokenFromMSALUsingSystemAssignedManagedIdentity(string resource)
        {
            LoggingUtilities.WriteLog("Attempting to acquire access token using system assigned managed identity auth.");

            var app = ManagedIdentityApplicationBuilder
                .Create(ManagedIdentityId.SystemAssigned)
                .Build();

            try
            {
                var result = await app.AcquireTokenForManagedIdentity(resource)
                    .ExecuteAsync();

                LoggingUtilities.WriteLog($"  Successfully acquired access token.");

                return result.AccessToken;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                throw;
            }
        }

        /// <summary>
        /// Gets AAD access token to Azure SQL using managed identity.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="identityClientId"></param>
        /// <returns></returns>
        public static async Task<string> GetSQLAccessTokenFromMSALUsingUserAssignedManagedIdentity(string resource, string identityClientId)
        {
            LoggingUtilities.WriteLog("Attempting to acquire access token using user assigned managed identity auth.");

            var app = ManagedIdentityApplicationBuilder
                .Create(ManagedIdentityId.WithUserAssignedClientId(identityClientId))
                .Build();

            try
            {
                var result = await app.AcquireTokenForManagedIdentity(resource)
                    .ExecuteAsync();

                LoggingUtilities.AddEmptyLine();
                LoggingUtilities.WriteLog($"    Successfully acquired access token.");

                return result.AccessToken;
            }
            catch (Exception ex)
            {
                HandleException(ex);

                throw;
            }
        }

        /// <summary>
        /// Creates a public client application using the authority.
        /// </summary>
        /// <param name="authority"></param>
        /// <returns></returns>
        private static IPublicClientApplication CreateClientApp(string authority)
        {
            return PublicClientApplicationBuilder.Create(AdoClientId)
                .WithAuthority(authority)
                .WithLogging(LogCallback, LogLevel.Verbose, true)
                .Build();
        }

        private static void LogCallback(LogLevel level, string message, bool containsPii)
        {
            LoggingUtilities.WriteLog($"[{level}] {(containsPii ? "[PII]" : "")} {message}");
        }

        /// <summary>
        /// Handles exceptions thrown by MSAL.
        /// </summary>
        /// <param name="ex"></param>
        private static void HandleException(Exception ex)
        {
            if (ex is MsalServiceException serviceException)
            {
                LoggingUtilities.WriteLog($"Service exception: {serviceException.Message}");
                LoggingUtilities.WriteLog($"Error code: {serviceException.ErrorCode}");
                LoggingUtilities.WriteLog($"HTTP status code: {serviceException.StatusCode}");
            }
            else if (ex is MsalClientException clientException)
            {
                LoggingUtilities.WriteLog($"Client exception: {clientException.Message}");
            }
            else
            {
                LoggingUtilities.WriteLog($"An unexpected error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates the input parameters.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        private static void ValidateInputParameters(string authority, string userId = null, string password = null)
        {
            if (string.IsNullOrEmpty(authority))
                throw new ArgumentException("Authority cannot be null or empty.", nameof(authority));

            if (userId != null && string.IsNullOrEmpty(userId))
                throw new ArgumentException("UserId cannot be null or empty.", nameof(userId));

            if (password != null && string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }
    }
}
