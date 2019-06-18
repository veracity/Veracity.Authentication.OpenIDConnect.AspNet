// <copyright file="VeracityPlatformService.cs" company="DNV GL Veracity">
//  Licensed under the MIT License.   
// </copyright>
// <summary>
//   Class that provides the ability to get an authentication header from Veracity
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Veracity.Authentication.OpenIDConnect.AspNet
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Identity.Client;

    /// <summary>
    /// Class that provides the ability to get an authentication header from Veracity
    /// </summary>
    public class VeracityPlatformService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VeracityPlatformService"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public VeracityPlatformService(HttpClient client)
        {
            client.BaseAddress = new Uri(VeracityIntegrationOptions.VeracityPlatformServiceUrl);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", VeracityIntegrationOptions.VeracityPlatformServiceKey);
            this.Client = client;
        }

        /// <summary>
        /// Gets the HTTP client which has the APIM subscription key header already added
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public HttpClient Client { get; }

        /// <summary>
        /// Gets the authentication header asynchronously
        /// </summary>
        /// <returns>An <see cref="AuthenticationHeaderValue"></see>/></returns>
        /// <exception cref="MsalUiRequiredException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync()
        {
            string accessToken = await this.GetAccessTokenAsync();
            return new AuthenticationHeaderValue("Bearer", accessToken);
        }

        /// <summary>
        /// Gets an access token asynchronously.
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetAccessTokenAsync()
        {
            IConfidentialClientApplication app = MSALAppBuilder.BuildConfidentialClientApplication();

            AuthenticationResult result;
            IEnumerable<IAccount> accounts = await app.GetAccountsAsync();
            string[] scopes = VeracityIntegrationOptions.VeracityPlatformServiceScopes.Split(' ');

            var accountList = accounts.ToList();
            var account = accountList.FirstOrDefault();

            try
            {
                // try to get an already cached token
                result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync().ConfigureAwait(false);
            }
            catch (MsalUiRequiredException ex)
            {
                // Cannot find any cache user in memory, you should sign out and login again.
                Debug.WriteLine($"Cannot find any cache user in memory {ex.Message}");
                throw;
            }

            return result.AccessToken;
        }
    }
}
