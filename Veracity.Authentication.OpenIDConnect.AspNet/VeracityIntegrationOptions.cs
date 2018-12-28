// <copyright file="VeracityIntegrationOptions.cs" company="DNV GL Veracity">
// Licensed under the Apache License, Version 2.0.   
// </copyright>
// <summary>
//   Defines the VeracityIntegrationOptions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Veracity.Authentication.OpenIDConnect
{
    using System.Configuration;

    public class VeracityIntegrationOptions
    {
        private const string TenantAddress = "dnvglb2cprod.onmicrosoft.com";

        private const string SignUpSignInPolicyId = "B2C_1A_SignInWithADFSIdp";

        private const string VeracityPlatformServiceBaseUrl = "https://api.veracity.com";

        private const string DefaultScope = "https://dnvglb2cprod.onmicrosoft.com/83054ebf-1d7b-43f5-82ad-b2bde84d7b75/user_impersonation";

        public static string ClientId => ConfigurationManager.AppSettings["veracity:ClientId"];

        public static string Tenant => ConfigurationManager.AppSettings["veracity:Tenant"] ?? TenantAddress;

        public static string RedirectUri => ConfigurationManager.AppSettings["veracity:RedirectUri"];

        public static string DefaultPolicy => ConfigurationManager.AppSettings["veracity:DefaultPolicy"] ?? SignUpSignInPolicyId;

        public static string ClientSecret => ConfigurationManager.AppSettings["veracity:ClientSecret"];

        public static string VeracityPlatformServiceUrl =>
            ConfigurationManager.AppSettings["veracity:VeracityPlatformServiceUrl"] ?? VeracityPlatformServiceBaseUrl;

        public static string VeracityPlatformServiceKey =>
            ConfigurationManager.AppSettings["veracity:APISubscriptionKey"];

        public static string VeracityPlatformServiceScopes =>
            ConfigurationManager.AppSettings["veracity:Scope"] ?? DefaultScope;

        public static string MetaDataAddress => $"https://login.microsoftonline.com/{Tenant}/v2.0/.well-known/openid-configuration?p={DefaultPolicy}";

        public static string Authority => $"https://login.microsoftonline.com/tfp/{Tenant}/{DefaultPolicy}/v2.0/.well-known/openid-configuration";
    }
}
