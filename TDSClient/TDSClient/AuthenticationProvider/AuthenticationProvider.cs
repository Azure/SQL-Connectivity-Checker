using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TDSClient.TDS.Client;

namespace TDSClient.AuthenticationProvider
{
    public class AuthenticationProvider
    {
        /// <summary>
        /// Type of authentication.
        /// </summary>
        public enum TDSAuthenticationType
        {
            SQLServerAuthentication,
            ADPassword,
            ADIntegrated,
            ADInteractive,
            ADManagedIdentity
        }

        /// <summary>
        /// Authentication type string to enum mapping.
        /// </summary>
        public static readonly Dictionary<string, TDSAuthenticationType> AuthTypeStringToEnum = new Dictionary<string, TDSAuthenticationType>
        {
            { "SQL Server Authentication", TDSAuthenticationType.SQLServerAuthentication },
            { "Active Directory Password", TDSAuthenticationType.ADPassword },
            { "Active Directory Integrated", TDSAuthenticationType.ADIntegrated },
            { "Active Directory Interactive", TDSAuthenticationType.ADInteractive },
            { "Active Directory Managed Identity", TDSAuthenticationType.ADManagedIdentity },
            { "Active Directory MSI", TDSAuthenticationType.ADManagedIdentity }
        };

        readonly string AuthenticationLibrary;
        readonly TDSAuthenticationType AuthenticationType;
        readonly string UserID;
        readonly string Password;
        readonly string AadAuthorityAudience;
        readonly string Resource;
        readonly string IdentityClientId;

        /// <summary>
        /// AuthenticationProvider constructor.
        /// </summary>
        /// <param name="authenticationLibrary"></param>
        /// <param name="authenticationType"></param>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <param name="aadAuthorityAudience"></param>
        /// <param name="resource"></param>
        public AuthenticationProvider(
            string authenticationLibrary,
            TDSAuthenticationType authenticationType,
            string userId,
            string password,
            string aadAuthorityAudience,
            string resource)
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
        /// <returns>Access token as a string</returns>
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
        /// <returns>Access token as a string</returns>
        private async Task<string> GetAccessTokenForIntegratedAuth()
        {
            return AuthenticationLibrary.Contains("MSAL") ?
                await MSALHelper.GetSQLAccessTokenFromMSALUsingIntegratedAuth(AadAuthorityAudience, Resource, UserID) :
                await ADALHelper.GetSQLAccessTokenFromADALUsingIntegratedAuth(AadAuthorityAudience, Resource);
        }

        /// <summary>
        /// Acquires access token for AAD username password authentication.
        /// </summary>
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
        /// <returns></returns>
        private async Task<string> GetAccessTokenForInteractiveAuth()
        {
            return await MSALHelper.GetSQLAccessTokenFromMSALInteractively(AadAuthorityAudience);
        }

        /// <summary>
        /// Acquires access token for AAD integrated authentication.
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetAccessTokenForMSIAuth()
        {
            return IdentityClientId != null ?
                await MSALHelper.GetSQLAccessTokenFromMSALUsingUserAssignedManagedIdentity(AadAuthorityAudience, IdentityClientId) :
                await MSALHelper.GetSQLAccessTokenFromMSALUsingSystemAssignedManagedIdentity(AadAuthorityAudience);
        }
    }
}
