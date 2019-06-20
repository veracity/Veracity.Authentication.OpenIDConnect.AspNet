// <copyright file="Startup.cs" company="DNV GL Veracity">
//  Licensed under the MIT License.   
// </copyright>
// <summary>
//   OWIN middleware startup class to configure OpenID settings
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Veracity.Authentication.OpenIDConnect.AspNet
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.Identity.Client;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.Owin.Extensions;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Cookies;
    using Microsoft.Owin.Security.Notifications;
    using Microsoft.Owin.Security.OpenIdConnect;
    using Owin;

    /// <summary>
    /// OWIN middleware startup class to configure OpenID settings. If using more than one OWIN startup class, this should be called by your own implementation first.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configurations the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        public void Configuration(IAppBuilder app)
        {
            this.ConfigureAuth(app);
        }

        /// <summary>
        /// Configures authentication.
        /// </summary>
        /// <param name="app">The application.</param>
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
                    PostLogoutRedirectUri = VeracityIntegrationOptions.PostLogoutRedirectUri,
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        AuthorizationCodeReceived = this.OnAuthorizationCodeReceivedAsync,
                        AuthenticationFailed = this.OnAuthenticationFailed
                    },
                    ResponseType = OpenIdConnectResponseType.CodeIdToken,
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        IssuerValidator = (issuer, token, tokenValidationParameters) =>
                        {
                            if (issuer.StartsWith(VeracityIntegrationOptions.Issuer, System.StringComparison.OrdinalIgnoreCase))
                            {
                                return issuer;
                            }

                            throw new SecurityTokenInvalidIssuerException("Invalid issuer");
                        },
                        NameClaimType = "UserId"
                    },
                    Scope = $"openid offline_access {VeracityIntegrationOptions.VeracityPlatformServiceScopes}"
                });

            // set the OWIN pipeline stage to Authenticate - this allows for authorization to be set in web.config 
            // e.g. To only allow authenticated users add the following
            // <authorization>
            //     <deny users = "?" />
            // </authorization>
            app.UseStageMarker(PipelineStage.Authenticate);
        }

        /// <summary>
        /// Called when an authorization code is received.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private async Task OnAuthorizationCodeReceivedAsync(AuthorizationCodeReceivedNotification context)
        {
            var claimsPrincipal = new ClaimsPrincipal(context.AuthenticationTicket.Identity);

            // Upon successful sign in, get the access token & cache it using MSAL
            IConfidentialClientApplication clientApp = MSALAppBuilder.BuildConfidentialClientApplication(claimsPrincipal);

            AuthenticationResult result = await clientApp
                .AcquireTokenByAuthorizationCode(VeracityIntegrationOptions.DefaultScope.Split(' '), context.Code)
                .ExecuteAsync();
        }

        /// <summary>
        /// Called when authentication fails.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <returns></returns>
        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();
            notification.Response.Redirect("/Error?message=" + notification.Exception.Message);
            return Task.FromResult(0);
        }
    }
}
