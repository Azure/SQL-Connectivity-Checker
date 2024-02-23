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
        /// Gets AAD access token using ADAL with username and password.
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
            try
            {
                LoggingUtilities.WriteLog($"  Acquiring access token using username and password.");
                AuthenticationContext authContext = new AuthenticationContext(authority);
                UserPasswordCredential userCredentials = new UserPasswordCredential(userId, password);

                AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientId, userCredentials);

                LoggingUtilities.WriteLog($"  Successfully acquired access token.");

                return result.AccessToken;
            }
            catch (AdalServiceException ex)
            {
                LoggingUtilities.WriteLog($"Service exception occurred when trying to acquire a JWT token: {ex.Message}");

                LoggingUtilities.WriteLog($"Error code: {ex.ErrorCode}");
                LoggingUtilities.WriteLog($"HTTP status code: {ex.StatusCode}");

                throw;
            }
            catch (AdalException ex)
            {
                // Handle client-related exceptions
                LoggingUtilities.WriteLog($"Client exception occurred when trying to acquire a JWT token: {ex.Message}");
                LoggingUtilities.WriteLog($"Error code: {ex.ErrorCode}");

                throw;
            }
            catch (Exception ex)
            {
                // An unexpected error occurred
                LoggingUtilities.WriteLog($"An unexpected error occurred when trying to acquire a JWT token: {ex.Message}");

                throw;
            }
        }

        /// <summary>
        /// Gets AAD access token using ADAL with integrated authentication.
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
            try
            {
                LoggingUtilities.WriteLog($"  Acquiring access token using integrated authentication.");
                AuthenticationContext authContext = new AuthenticationContext(authority);

                AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientId, new UserCredential());

                LoggingUtilities.WriteLog($"  Successfully acquired access token.");

                return result.AccessToken;
            }
            catch (AdalServiceException ex)
            {
                LoggingUtilities.WriteLog($"Service exception: {ex.Message}");

                LoggingUtilities.WriteLog($"Error code: {ex.ErrorCode}");
                LoggingUtilities.WriteLog($"HTTP status code: {ex.StatusCode}");

                throw;
            }
            catch (AdalException ex)
            {
                // Handle client-related exceptions
                LoggingUtilities.WriteLog($"Client exception: {ex.Message}");
                LoggingUtilities.WriteLog($"Error code: {ex.ErrorCode}");

                throw;
            }
            catch (Exception ex)
            {
                // An unexpected error occurred
                LoggingUtilities.WriteLog($"An unexpected error occurred: {ex.Message}");

                throw;
            }
        }
    }
}