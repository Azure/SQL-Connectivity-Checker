using System;
using System.Threading.Tasks;

using TDSClient.TDS.Client;

namespace TDSClient.AuthenticationProvider
{
    public class AuthenticationProvider
    {
        readonly string AuthenticationLibrary;
        readonly TDSAuthenticationType AuthenticationType;
        readonly string UserID;
        readonly string Password;
        readonly string AadAuthorityAudience;
        readonly string Resource;
        readonly string IdentityClientId;

        public AuthenticationProvider(string authenticationLibrary, TDSAuthenticationType authenticationType, string userId, string password, string aadAuthorityAudience, string resource)
        {
            AuthenticationLibrary = authenticationLibrary;
            AuthenticationType = authenticationType;
            UserID = userId;
            Password = password;
            AadAuthorityAudience = aadAuthorityAudience;
            Resource = resource;
        }

        /// <summary>
        /// Acquires JWT Access token.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <param name="clientID"></param>
        /// <returns></returns>
        public async Task<string> GetJWTAccessToken()
        {
            string accessToken = null;

            switch (AuthenticationType)
            {
                case TDSAuthenticationType.ADIntegrated:
                    accessToken = await GetAccessTokenForIntegratedAuth();
                    break;
                case TDSAuthenticationType.ADInteractive:
                    accessToken = await GetAccessTokenForInteractiveAuth();
                    break;
                case TDSAuthenticationType.ADPassword:
                    accessToken = await GetAccessTokenForUsernamePassword();
                    break;
                case TDSAuthenticationType.ADManagedIdentity:
                    accessToken = await GetAccessTokenForMSIAuth();
                    break;
            }

            return accessToken;
        }

        /// <summary>
        /// Acquires access token for AAD integrated authentication.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <param name="clientID"></param>
        /// <returns></returns>
        private async Task<string> GetAccessTokenForIntegratedAuth()
        {
            return AuthenticationLibrary.Contains("MSAL") ?
                await MSALHelper.GetSQLAccessTokenFromMSALUsingIntegratedAuth(AadAuthorityAudience, Resource, UserID) :
                await ADALHelper.GetSQLAccessTokenFromADALUsingIntegratedAuth(AadAuthorityAudience, Resource);
        }

        /// <summary>
        /// Acquires access token for AAD username password authentication.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <param name="clientID"></param>
        /// <returns></returns>
        private async Task<string> GetAccessTokenForUsernamePassword()
        {
            return AuthenticationLibrary.Contains("MSAL") ?
               await MSALHelper.GetSQLAccessTokenFromMSALUsingUsernamePassword(AadAuthorityAudience, Resource, UserID, Password) :
               throw new Exception("Username password authentication is not supported by ADAL.");
        }

        /// <summary>
        /// Acquires access token for AAD integrated authentication.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <param name="clientID"></param>
        /// <returns></returns>
        private async Task<string> GetAccessTokenForInteractiveAuth()
        {
            return await MSALHelper.GetSQLAccessTokenFromMSALInteractively(AadAuthorityAudience);
        }

        /// <summary>
        /// Acquires access token for AAD integrated authentication.
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="resource"></param>
        /// <param name="clientID"></param>
        /// <returns></returns>
        private async Task<string> GetAccessTokenForMSIAuth()
        {
            return IdentityClientId != null ?
                await MSALHelper.GetSQLAccessTokenFromMSALUsingUserAssignedManagedIdentity(AadAuthorityAudience, IdentityClientId) :
                await MSALHelper.GetSQLAccessTokenFromMSALUsingSystemAssignedManagedIdentity(AadAuthorityAudience);
        }
    }
}
