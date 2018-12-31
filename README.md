# Veracity.Authentication.OpenIDConnect.AspNet [![NuGet version](https://badge.fury.io/nu/Veracity.Authentication.OpenIDConnect.AspNet.svg)](https://badge.fury.io/nu/Veracity.Authentication.OpenIDConnect.AspNet)
Veracity authentication library for applications based on ASP.NET Framework(.NET Framework Version >= 4.6.1)
## For new applications
We highly recommanded you checkout https://github.com/veracity/Veracity.Authentication.OpenIDConnect.AspNetCore to use Veracity app generator to generator .net core solution. 

If we insist to use .net framwork solution or you want to migrate your solution from legcy SAML based authentication to Azure B2C OpenId, download the sample code and checkout usage in next section. 

## For existing applications
1. Make sure that your .NET Framework version >= 4.6.1. If not, [download the latest version](https://www.microsoft.com/net/download).
2. Go to the NuGet package manager and install `Veracity.Authentication.OpenIDConnect.AspNet`
3. Update the `web.config` file with information you get after registering your application, go to Veracity support page request subscription key for platform services. 
```XML
    <add key="veracity:ClientId" value="" />
    <add key="veracity:ClientSecret" value="" />
    <add key="veracity:RedirectUri" value="" />
    <add key="veracity:APISubscriptionKey" value="" />
```
Please note that we will add the above appsetting during the installation, if you cannot find it, please add those into appSetting. The following setting make sure you app connect to Veracity authentication library successfully. 
```XML
    <add key="owin:AppStartup" value="Veracity.Authentication.OpenIDConnect.AspNet.Startup" />
```
4. Create AccountController if you do not have and put following code into `AccountController.cs` , please refer Demo https://github.com/veracity/Veracity.Authentication.OpenIDConnect.AspNet/blob/master/Sample/Demo/Demo/Controllers/AccountController.cs 
```C#
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
                Request.GetOwinContext().Authentication.GetAuthenticationTypes();
            }
        }
```
5. Download sample to find out how to call Veracity Platform service API, please refer https://github.com/veracity/Veracity.Authentication.OpenIDConnect.AspNet/blob/master/Sample/Demo/Demo/Controllers/HomeController.cs 
```C#
   [Authorize]
        public async Task<ActionResult> CallApiAsync()
        {
            var service = new VeracityPlatformService(client, this.HttpContext);
            var request = new HttpRequestMessage(HttpMethod.Get, "/platform/my/profile");
            request.Headers.Authorization = await service.GetAuthenticationHeaderAsync();
            var response = await client.SendAsync(request);
            ViewData["Payload"] = await response.Content.ReadAsStringAsync();
            return View();
        }
```
we suggest you do not create HttpClient every time, try to manage the client pool for better performance.

## Integrate with the Veracity policy service (check terms and conditions) and check the service subscription
Veracity will integrate the policy service into identity provider, but before we have done that, you need to check the policy services in your code manually before the user lands on the home page.  
```C#
        [Authorize]
        public async Task<IActionResult> ValidatePolicy()
        {
            var service = new VeracityPlatformService(client, this.HttpContext);
            var request = new HttpRequestMessage(HttpMethod.Get, "/my/policies/{serviceId}/validate()");
            request.Headers.Authorization = await service.GetAuthenticationHeaderAsync();
            var response = await client.SendAsync(request);
            switch (response.StatusCode)
            {
                    case HttpStatusCode.NoContent:
                        break;
                    case HttpStatusCode.NotAcceptable:
                       var content = await response.Content.ReadAsStringAsync();
                       //you need to grab the url from the respnse and redirect user to this address, Veracity will handle the following stuff. 
                       return Redirect(content.url);
                    default:
                        responseString = $"Error calling API. StatusCode=${response.StatusCode}";
                        break;
            }    
            return View();
        }
```

