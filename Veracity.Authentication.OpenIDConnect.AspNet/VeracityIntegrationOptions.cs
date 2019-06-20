// <copyright file="VeracityIntegrationOptions.cs" company="DNV GL Veracity">
// Licensed under the MIT License.   
// </copyright>
// <summary>
//   Defines the VeracityIntegrationOptions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Veracity.Authentication.OpenIDConnect.AspNet
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Class that defines the information required to use Azure B2C for the Veracity V3 tenant
    /// </summary>
    public class VeracityIntegrationOptions
    {
        /// <summary>
        /// The tenant address
        /// </summary>
        private const string TenantAddress = "dnvglb2cprod.onmicrosoft.com";

        /// <summary>
        /// The sign up sign in policy identifier
        /// </summary>
        private const string SignUpSignInPolicyId = "B2C_1A_SignInWithADFSIdp";

        /// <summary>
        /// The veracity platform service base URL
        /// </summary>
        private const string VeracityPlatformServiceBaseUrl = "https://api.veracity.com";

        /// <summary>
        /// The default scope
        /// </summary>
        public static string DefaultScope = "https://dnvglb2cprod.onmicrosoft.com/83054ebf-1d7b-43f5-82ad-b2bde84d7b75/user_impersonation";

        /// <summary>
        /// Gets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public static string ClientId => ConfigurationManager.AppSettings["veracity:ClientId"];

        /// <summary>
        /// Gets the tenant.
        /// </summary>
        /// <value>
        /// The tenant.
        /// </value>
        public static string Tenant => ConfigurationManager.AppSettings["veracity:Tenant"] ?? TenantAddress;

        /// <summary>
        /// Gets the redirect URI.
        /// </summary>
        /// <value>
        /// The redirect URI.
        /// </value>
        public static string RedirectUri => ConfigurationManager.AppSettings["veracity:RedirectUri"];

        /// <summary>
        /// Gets the default policy.
        /// </summary>
        /// <value>
        /// The default policy.
        /// </value>
        public static string DefaultPolicy => ConfigurationManager.AppSettings["veracity:DefaultPolicy"] ?? SignUpSignInPolicyId;

        /// <summary>
        /// Gets the client secret.
        /// </summary>
        /// <value>
        /// The client secret.
        /// </value>
        public static string ClientSecret => ConfigurationManager.AppSettings["veracity:ClientSecret"];

        /// <summary>
        /// Gets the veracity platform service URL.
        /// </summary>
        /// <value>
        /// The veracity platform service URL.
        /// </value>
        public static string VeracityPlatformServiceUrl =>
            ConfigurationManager.AppSettings["veracity:VeracityPlatformServiceUrl"] ?? VeracityPlatformServiceBaseUrl;

        /// <summary>
        /// Gets the veracity platform service key.
        /// </summary>
        /// <value>
        /// The veracity platform service key.
        /// </value>
        public static string VeracityPlatformServiceKey =>
            ConfigurationManager.AppSettings["veracity:APISubscriptionKey"];

        /// <summary>
        /// Gets the veracity platform service scopes.
        /// </summary>
        /// <value>
        /// The veracity platform service scopes.
        /// </value>
        public static string VeracityPlatformServiceScopes => ConfigurationManager.AppSettings["veracity:Scope"] ?? DefaultScope;

        /// <summary>
        /// Gets the meta data address.
        /// </summary>
        /// <value>
        /// The meta data address.
        /// </value>
        public static string MetaDataAddress => $"https://login.microsoftonline.com/{Tenant}/v2.0/.well-known/openid-configuration?p={DefaultPolicy}";

        /// <summary>
        /// Gets the authority.
        /// </summary>
        /// <value>
        /// The authority.
        /// </value>
        public static string Authority => $"https://login.microsoftonline.com/tfp/{Tenant}/{DefaultPolicy}/v2.0/.well-known/openid-configuration";

        /// <summary>
        /// Gets the post logout redirect URI.
        /// </summary>
        /// <value>
        /// The post logout redirect URI.
        /// </value>
        public static string PostLogoutRedirectUri => ConfigurationManager.AppSettings["veracity:PostLogoutRedirectUri"] ?? RedirectUri;

        /// <summary>
        /// Gets the token issuer for validation of received tokens
        /// </summary>
        /// <value>
        /// The expected token issuer string.
        /// </value>
        public static string Issuer => ConfigurationManager.AppSettings["veracity:Issuer"] ?? "https://login.microsoftonline.com/a68572e3-63ce-4bc1-acdc-b64943502e9d";
    }
}
