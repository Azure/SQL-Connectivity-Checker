//  ---------------------------------------------------------------------------
//  <copyright file="MSALHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Abstractions;

using Microsoft.Identity.Client.AppConfig;
using TDSClient.TDS.Utilities;

namespace TDSClient.MSALHelper
{
    public class MSALHelper
    {
        private static readonly string AdoClientId = "4d079b4c-cab7-4b7c-a115-8fd51b6f8239";

        /// <summary>
        /// Gets AAD access token to Azure SQL using user credentials (username and password).
        /// </summary>
        /// <param name="authoritystring"></param>
        /// <param name="clientId"></param>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<string> GetSQLAccessTokenFromMSALUsingUsernamePassword(string authority, string resource, string userId, string password)
        {
            // Validate input parameters
            if (string.IsNullOrEmpty(authority))
                throw new ArgumentException("Authority cannot be null or empty.", nameof(authority));
            
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("UserId cannot be null or empty.", nameof(userId));
            
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));

            var app = CreateClientApp(authority);

            string[] scopes = new[] { resource + "/.default"};

  
                LoggingUtilities.WriteLog("Attempting to acquire access token using username and password.");
                var result = await app.AcquireTokenByUsernamePassword(scopes, userId, password)
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                LoggingUtilities.WriteLog("Successfully acquired access token.");
                return result.AccessToken;
        }

        /// <summary>
        /// Gets AAD access token to Azure SQL using integrated authentication.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static async Task<string> GetSQLAccessTokenFromMSALUsingIntegratedAuth(string authority, string resource, string userId)
        {
            // Validate input parameters
            if (string.IsNullOrEmpty(authority))
                throw new ArgumentException("Authority cannot be null or empty.", nameof(authority));

            LoggingUtilities.WriteLog("Attempting to acquire access token using integrated auth.");

            var app = CreateClientApp(authority);

            string[] scopes = new[] { resource + "/.default" };

            try
            {
                var result = userId == null ? await app.AcquireTokenByIntegratedWindowsAuth(scopes)
                    .ExecuteAsync(CancellationToken.None)
                    .ConfigureAwait(false) : await app.AcquireTokenByIntegratedWindowsAuth(scopes)
                    .WithUsername(userId)
                    .ExecuteAsync(CancellationToken.None)
                    .ConfigureAwait(false);

                LoggingUtilities.WriteLog("Successfully acquired access token.");
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
                LoggingUtilities.WriteLog($"An unexpected error occurred: {ex.Message}");

                throw;
            }
        }

        /// <summary>
        /// Gets AAD access token to Azure SQL using interactive authentication.
        /// </summary>
        /// <param name="authority"></param>
        /// <returns></returns>
        public static async Task<string> GetSQLAccessTokenFromMSALInteractively(string resource)
        {
            // Validate input parameters
            if (string.IsNullOrEmpty(resource))
                throw new ArgumentException("Authority cannot be null or empty.", nameof(resource));

            string[] scopes = new string[] { resource + "/.default" };

            LoggingUtilities.WriteLog("Attempting to acquire access token using interactive auth.");
            var app = PublicClientApplicationBuilder.Create(AdoClientId)
                .WithRedirectUri("http://localhost")
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
        /// Gets AAD access token to Azure SQL using managed identity.
        /// </summary>
        /// <param name="authority"></param>
        /// <returns></returns>
        public static async Task<string> GetSQLAccessTokenFromMSALUsingSystemAssignedManagedIdentity(string resource)
        {
            LoggingUtilities.WriteLog("Attempting to acquire access token using system assigned managed identity auth.");

            var app = ManagedIdentityApplicationBuilder.Create(ManagedIdentityId.SystemAssigned)
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
        /// <param name="authority"></param>
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
        /// 
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="clientId"></param>
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
            // Customize how you handle logs here
            LoggingUtilities.WriteLog($"[{level}] {(containsPii ? "[PII]" : "")} {message}");
        }

        /// <summary>
        /// 
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
    }

    // public class MsalLogger : IIdentityLogger
    // {
    //     public EventLogLevel MinLogLevel { get; }

    //     public MsalLogger()
    //     {
    //         //Retrieve the log level from an environment variable
    //         var msalEnvLogLevel = Environment.GetEnvironmentVariable("MSAL_LOG_LEVEL");

    //         if (Enum.TryParse(msalEnvLogLevel, out EventLogLevel msalLogLevel))
    //         {
    //             MinLogLevel = msalLogLevel;
    //         }
    //         else
    //         {
    //             //Recommended default log level
    //             MinLogLevel = EventLogLevel.Verbose;
    //         }
    //     }

    //     public bool IsEnabled(EventLogLevel eventLogLevel)
    //     {
    //         return eventLogLevel <= MinLogLevel;
    //     }

    //     public void Log(LogEntry entry)
    //     {
    //         //Log Message here:
    //         LoggingUtilities.WriteLog(entry.Message);
    //     }
    // }
}