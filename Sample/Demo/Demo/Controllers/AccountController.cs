using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Demo.Controllers
{
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.OpenIdConnect;

    public class AccountController : Controller
    {
        // GET: Account
        [HttpGet]
        public void SignIn()
        {
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        [HttpGet]
        public void SignOut()
        {
            if (Request.IsAuthenticated)
            {
                IEnumerable<AuthenticationDescription> authTypes = this.HttpContext.GetOwinContext().Authentication.GetAuthenticationTypes();
                this.HttpContext.GetOwinContext().Authentication.SignOut(authTypes.Select(t => t.AuthenticationType).ToArray()); 
            }
        }
    }
}