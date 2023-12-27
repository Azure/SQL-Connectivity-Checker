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
        /// Gets AAD access token to Azure SQL using user credentials
        /// </summary>
        public static async Task<string> GetSQLAccessTokenFromMSALUsingUsernamePassword(string authority, string resource, string clientId, string userId, string password)
        {
            IPublicClientApplication app;
            app = PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority(authority)
                .Build();
            string[] scopes = new[] { "https://database.windows.net/.default" };

            try
            {
                var result = await app.AcquireTokenByUsernamePassword(scopes, userId, password)
                    .ExecuteAsync();

                LoggingUtilities.WriteLog($"  Successfully acquired access token.");
                return result.AccessToken;
            }
            catch (MsalServiceException ex)
            {
                Console.WriteLine($"Service exception: {ex.Message}");

                Console.WriteLine($"Error code: {ex.ErrorCode}");
                Console.WriteLine($"HTTP status code: {ex.StatusCode}");

                throw;
            }
            catch (MsalClientException ex)
            {
                // MSAL client exception occurred
                Console.WriteLine($"Client exception: {ex.Message}");

                throw;
            }
            catch (Exception ex)
            {
                // An unexpected error occurred
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");

                throw;
            }
        }

        /// <summary>
        /// Gets AAD access token to Azure SQL using user credentials
        /// </summary>
        public static async Task<string> GetSQLAccessTokenFromMSALUsingIntegratedAuth(string authority, string resource, string clientId)
        {
            IPublicClientApplication app;
            app = PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority(authority)
                .Build();
            string[] scopes = new[] { "https://database.windows.net/.default" };

            try
            {
                var result = await app.AcquireTokenByIntegratedWindowsAuth(scopes)
                    .ExecuteAsync(CancellationToken.None);

                LoggingUtilities.WriteLog($"  Successfully acquired access token.");

                return result.AccessToken;
            }
            catch (MsalServiceException ex)
            {
                Console.WriteLine($"Service exception: {ex.Message}");

                Console.WriteLine($"Error code: {ex.ErrorCode}");
                Console.WriteLine($"HTTP status code: {ex.StatusCode}");

                throw;
            }
            catch (MsalClientException ex)
            {
                // MSAL client exception occurred
                Console.WriteLine($"Client exception: {ex.Message}");

                throw;
            }
            catch (Exception ex)
            {
                // An unexpected error occurred
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");

                throw;
            }
        }
    }
}