# Veracity.Authentication.OpenIDConnect.AspNet [![NuGet version]
(https://badge.fury.io/nu/Veracity.Authentication.OpenIDConnect.AspNet.svg)](https://badge.fury.io/nu/Veracity.Authentication.OpenIDConnect.AspNet)
Veracity authentication library for applications based on ASP.NET Framework(.NET Framework Version >= 4.6.1)
## For new applications
1. Go to https://developer.veracity.com/ and enroll as developer
2. Create your project and applications using the developer self-service
3. Get  integration information through email which includes client ID etc. 
4. Go to https://developer.veracity.com/doc/create-veracity-app and see the instructions for creating Veracity apps using the Veracity App Generator(https://github.com/veracity/generator-veracity)
5. Update the Veracity appSettings in the `web.config` file
6. Run the application 

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

