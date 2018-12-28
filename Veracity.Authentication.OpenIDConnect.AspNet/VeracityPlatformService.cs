// <copyright file="VeracityPlatformService.cs" company="DNV GL Veracity">
//  Licensed under the Apache License, Version 2.0.   
// </copyright>
// <summary>
//   Defines the VeracityPlatformService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Veracity.Authentication.OpenIDConnect
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Authentication;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web;

    using Microsoft.Identity.Client;

    public class VeracityPlatformService
    {
        private readonly HttpContextBase _httpContext;

        public VeracityPlatformService(HttpClient client, HttpContextBase httpContext)
        {
            client.BaseAddress = new Uri(VeracityIntegrationOptions.VeracityPlatformServiceUrl);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", VeracityIntegrationOptions.VeracityPlatformServiceKey);
            this.Client = client;
            this._httpContext = httpContext;
        }

        public HttpClient Client { get; }

        public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync()
        {
            var accessToken = await this.GetAccessTokenAsync();
            return new AuthenticationHeaderValue("Bearer", accessToken);
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var signedInUserId = ClaimsPrincipal.Current.FindFirst("UserId").Value;
            var scope = VeracityIntegrationOptions.VeracityPlatformServiceScopes.Split(' ');
            TokenCache userTokenCache = new MSALSessionCache(signedInUserId, this._httpContext).GetMsalCacheInstance();
            ConfidentialClientApplication cca = new ConfidentialClientApplication(
                VeracityIntegrationOptions.ClientId,
                VeracityIntegrationOptions.Authority,
                VeracityIntegrationOptions.RedirectUri,
                new ClientCredential(VeracityIntegrationOptions.ClientSecret),
                userTokenCache,
                null);
            try
            {
                IEnumerable<IAccount> accounts = await cca.GetAccountsAsync();
                IAccount firstAccount = accounts.FirstOrDefault();
                var result = await cca.AcquireTokenSilentAsync(
                                 scope,
                                 firstAccount,
                                 VeracityIntegrationOptions.Authority,
                                 false);
                return result.AccessToken;
            }
            catch (MsalUiRequiredException)
            {
                // Cannot find any cache user in memory, you should sign out and login again.
                throw new AuthenticationException("Cannot find login user credential, please sign out and login again");
            }
        }
    }
}
