﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        /// Type of authentication library.
        /// </summary>
        public enum TDSAuthenticationLibrary
        {
            ADAL,
            MSAL
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

        /// <summary>
        /// Authentication type string to enum mapping.
        /// </summary>
        public static readonly Dictionary<string, TDSAuthenticationLibrary> AuthLibStringToEnum = new Dictionary<string, TDSAuthenticationLibrary>
        {
            { "ADAL", TDSAuthenticationLibrary.ADAL },
            { "MSAL", TDSAuthenticationLibrary.MSAL }
        };

        private readonly TDSAuthenticationLibrary AuthenticationLibrary;
        private readonly TDSAuthenticationType AuthenticationType;
        private readonly string UserID;
        private readonly string Password;
        private readonly string AadAuthorityAudience;
        private readonly string Resource;
        private readonly string IdentityClientId;

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
            TDSAuthenticationLibrary authenticationLibrary,
            TDSAuthenticationType authenticationType,
            string userId,
            string password,
            string aadAuthorityAudience,
            string resource,
            string identityClientId = null)
        {
            AuthenticationLibrary = authenticationLibrary;
            AuthenticationType = authenticationType;
            UserID = userId;
            Password = password;
            AadAuthorityAudience = aadAuthorityAudience;
            Resource = resource;
            IdentityClientId = identityClientId;
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
            return AuthenticationLibrary == TDSAuthenticationLibrary.MSAL ?
                await MSALHelper.GetSQLAccessTokenFromMSALUsingIntegratedAuth(AadAuthorityAudience, Resource, UserID) :
                await ADALHelper.GetSQLAccessTokenFromADALUsingIntegratedAuth(AadAuthorityAudience, Resource);
        }

        /// <summary>
        /// Acquires access token for AAD username password authentication.
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetAccessTokenForUsernamePassword()
        {
            return AuthenticationLibrary == TDSAuthenticationLibrary.MSAL ?
               await MSALHelper.GetSQLAccessTokenFromMSALUsingUsernamePassword(AadAuthorityAudience, Resource, UserID, Password) :
               throw new Exception("Username password authentication is not supported by ADAL.");
        }

        /// <summary>
        /// Acquires access token for AAD integrated authentication.
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetAccessTokenForInteractiveAuth()
        {
            return await MSALHelper.GetSQLAccessTokenFromMSALInteractively(Resource, AadAuthorityAudience);
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
