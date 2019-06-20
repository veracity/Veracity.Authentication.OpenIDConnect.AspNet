// <copyright file="MsalAppBuilder.cs" company="DNV GL Veracity">
//  Licensed under the MIT License.   
// </copyright>
// <summary>
//   Static helper methods for creating confidential client applications and managing the token cache
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Veracity.Authentication.OpenIDConnect.AspNet
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.Identity.Client;

    /// <summary>
    /// Static helper methods for creating confidential client applications and managing the token cache
    /// </summary>
    public static class MSALAppBuilder
    {
        /// <summary>
        /// Builds the confidential client application.
        /// </summary>
        /// <returns></returns>
        public static IConfidentialClientApplication BuildConfidentialClientApplication()
        {
            return BuildConfidentialClientApplication(ClaimsPrincipal.Current);
        }

        /// <summary>
        /// Builds the confidential client application.
        /// </summary>
        /// <param name="currentUser">The current user.</param>
        /// <returns></returns>
        public static IConfidentialClientApplication BuildConfidentialClientApplication(ClaimsPrincipal currentUser)
        {
            var principal = currentUser ?? ClaimsPrincipal.Current;

            IConfidentialClientApplication clientApplication = ConfidentialClientApplicationBuilder
                .Create(VeracityIntegrationOptions.ClientId)
                .WithClientSecret(VeracityIntegrationOptions.ClientSecret)
                .WithRedirectUri(VeracityIntegrationOptions.RedirectUri)
                .WithAuthority(new Uri(VeracityIntegrationOptions.Authority))
                .Build();

            if (principal != null)
            {
                // After the ConfidentialClientApplication is created, we overwrite its default UserTokenCache with our implementation
                MSALPerUserMemoryTokenCache userTokenCache = new MSALPerUserMemoryTokenCache(clientApplication.UserTokenCache, principal);
            }

            return clientApplication;
        }

        /// <summary>
        /// Clears the user token cache.
        /// </summary>
        /// <returns></returns>
        public static async Task ClearUserTokenCache()
        {
            IConfidentialClientApplication clientApplication = ConfidentialClientApplicationBuilder
                .Create(VeracityIntegrationOptions.ClientId)
                .WithClientSecret(VeracityIntegrationOptions.ClientSecret)
                .WithRedirectUri(VeracityIntegrationOptions.RedirectUri)
                .WithAuthority(new Uri(VeracityIntegrationOptions.Authority))
                .Build();

            // We only clear the user's tokens.
            MSALPerUserMemoryTokenCache userTokenCache = new MSALPerUserMemoryTokenCache(clientApplication.UserTokenCache);

            var msalAccountId = ClaimsPrincipal.Current.GetMsalAccountId();

            var userAccount = await clientApplication.GetAccountAsync(msalAccountId);

            // remove all the tokens in the cache for the specified account
            await clientApplication.RemoveAsync(userAccount);

            // clear the client applications token cache copy of the users token cache
            userTokenCache.Clear();
        }
    }
}