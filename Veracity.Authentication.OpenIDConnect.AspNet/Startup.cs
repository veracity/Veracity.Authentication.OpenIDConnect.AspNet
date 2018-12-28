// <copyright file="Startup.cs" company="DNV GL Veracity">
//  Licensed under the Apache License, Version 2.0.   
// </copyright>
// <summary>
//   Defines the Startup type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Veracity.Authentication.OpenIDConnect.AspNet
{
    using System.Threading.Tasks;
    using System.Web;

    using Microsoft.Identity.Client;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Cookies;
    using Microsoft.Owin.Security.Notifications;
    using Microsoft.Owin.Security.OpenIdConnect;

    using Owin;

    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            this.ConfigureAuth(app);
        }

        private void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    // Generate the metadata address using the tenant and policy information
                    MetadataAddress = VeracityIntegrationOptions.MetaDataAddress,
                    Authority = VeracityIntegrationOptions.Authority,
                    ClientId = VeracityIntegrationOptions.ClientId,
                    RedirectUri = VeracityIntegrationOptions.RedirectUri,
                    PostLogoutRedirectUri = VeracityIntegrationOptions.RedirectUri,
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        AuthorizationCodeReceived = this.OnAuthorizationCodeReceivedAsync,
                        AuthenticationFailed = this.AuthenticationFailed
                    },
                    ResponseType = OpenIdConnectResponseType.CodeIdToken,
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "UserId"
                    },
                    Scope = $"openid offline_access {VeracityIntegrationOptions.VeracityPlatformServiceScopes}"
                });
        }

        private async Task OnAuthorizationCodeReceivedAsync(AuthorizationCodeReceivedNotification context)
        {
            var code = context.Code;
            string signedInUserId = context.AuthenticationTicket.Identity.FindFirst("UserId").Value;
            TokenCache userTokenCache = new MSALSessionCache(
                    signedInUserId,
                    context.OwinContext.Environment["System.Web.HttpContextBase"] as HttpContextBase)
                .GetMsalCacheInstance();
            var cca = new ConfidentialClientApplication(
                VeracityIntegrationOptions.ClientId,
                VeracityIntegrationOptions.Authority,
                VeracityIntegrationOptions.RedirectUri,
                new ClientCredential(VeracityIntegrationOptions.ClientSecret),
                userTokenCache,
                null);

            await cca.AcquireTokenByAuthorizationCodeAsync(code, VeracityIntegrationOptions.VeracityPlatformServiceScopes.Split(' '));
        }

        private Task AuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();
            notification.Response.Redirect("/Error?message=" + notification.Exception.Message);
            return Task.FromResult(0);
        }


    }
}
