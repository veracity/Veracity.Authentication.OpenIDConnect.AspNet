# Veracity.Authentication.OpenIDConnect.AspNet [![NuGet version](https://badge.fury.io/nu/Veracity.Authentication.OpenIDConnect.AspNet.svg)](https://badge.fury.io/nu/Veracity.Authentication.OpenIDConnect.AspNet)
Veracity authentication library for applications based on ASP.NET Framework (requires .NET Framework Version >= 4.7.1)
## For new applications
We highly recommend that you checkout https://github.com/veracity/Veracity.Authentication.OpenIDConnect.AspNetCore to use the Veracity app generator to generate a .net core solution. 

If you need to use a .net framwork solution or you want to migrate your solution from legcy SAML based authentication to Azure B2C OpenId, download the sample code and checkout usage in next section. 

## For existing applications
1. Make sure that your .NET Framework version >= 4.7.1. If not, [download the latest version](https://www.microsoft.com/net/download).
2. Go to the NuGet package manager and install `Veracity.Authentication.OpenIDConnect.AspNet`
3. Update the `web.config` file with the information you get after registering your application, go to the Veracity support page and request a subscription key for platform services.
```XML
    <add key="veracity:ClientId" value="" />
    <add key="veracity:ClientSecret" value="" />
    <add key="veracity:RedirectUri" value="" />
    <add key="veracity:PostLogoutRedirectUri" value="" />
    <add key="veracity:APISubscriptionKey" value="" />
```
The package will add the above app setting keys during installation, if you cannot find them, please add them manually. The following setting makes sure that your app can connect to the Veracity authentication library successfully. 
```XML
    <add key="owin:AppStartup" value="Veracity.Authentication.OpenIDConnect.AspNet.Startup" />
```
Note that it is only possible to have one "owin:AppStartup" entry. If your application needs to use owin:AppStartup, then your implementation must call the veracity library first. The following snippet should be insterted as the first thing in your AppStartup class
```C#
public partial class Startup
    {       
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // there can be only one OWIN Startup class, so we have to manually call the one provided by the Veracity authentication library
            var veracityStartup = new VeracityOpenId.Startup();
            veracityStartup.Configuration(app);

			...
			...
```
4. Create an AccountController if you don't have one and put following code into `AccountController.cs` , please refer to the Demo https://github.com/veracity/Veracity.Authentication.OpenIDConnect.AspNet/blob/master/Sample/Demo/Demo/Controllers/AccountController.cs 
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
            }
        }
```
5. Download sample to find out how to call Veracity Platform service API, please refer https://github.com/veracity/Veracity.Authentication.OpenIDConnect.AspNet/blob/master/Sample/Demo/Demo/Controllers/HomeController.cs 
```C#
   [Authorize]
        public async Task<ActionResult> CallApiAsync()
        {
            var service = new VeracityPlatformService(client, this.HttpContext);
            var request = new HttpRequestMessage(HttpMethod.Get, "/Veracity/Services/my/profile");
            // Calling data fabric API
            // var request = new HttpRequestMessage(HttpMethod.Get, "/veracity/datafabric/data/api/1/resources");
            request.Headers.Authorization = await service.GetAuthenticationHeaderAsync();
            var response = await client.SendAsync(request);
            ViewData["Payload"] = await response.Content.ReadAsStringAsync();
            return View();
        }
```
We suggest that you don't create a new HttpClient every time, try to manage the client pool for better performance.

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

## Web.config encryption considerations
If it's not possible to use a secure data store such as Azure KeyVault for the Veracity secret information, you may decide to encrypt part of your web.config.
This is typically achieved with a new configuration section. The following example is what such a section may look like **before** its encrypted:
```xml
<secureAppSettings>
	<add key="veracity:clientId" value="xxx" />
	...
</secureAppSettings>
```
However this will not be picked up automatically as it isn't in `<appSettings>`

There is a custom configuration builder included in the package which when activated will transpose any configuration section back into `<appSettings>`.
Add the following to your `web.config` to use this feature.

```xml
<configuration>
  <configSections>
    <section name="secureAppSettings" type="System.Configuration.NameValueSectionHandler, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089" />
    <section name="configBuilders" type="System.Configuration.ConfigurationBuildersSection, System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" restartOnExternalChanges="false" requirePermission="false" />
  </configSections>

  <configBuilders>
    <builders>      
      <add name="SecureAppSettings" type="Veracity.Authentication.OpenIDConnect.AspNet.SecureAppSettingsConfigurationBuilder, Veracity.Authentication.OpenIDConnect.AspNet, Version=1.0.0.0, Culture=neutral" />
    </builders>
  </configBuilders>

  <appSettings configBuilders="SecureAppSettings">
    <add key="veracity:ClientId" value="placeholder for value from configuration builder" />
    <add key="veracity:ClientSecret" value="placeholder for value from configuration builder" />
    <add key="veracity:APISubscriptionKey" value="placeholder for value from configuration builder" />    
  </appSettings>

  <secureAppSettings>
    <!-- these are the values **before** encryption -->
    <add key="veracity:ClientId" value="secret value to be encrypted" />
    <add key="veracity:ClientSecret" value="secret value to be encrypted" />
    <add key="veracity:APISubscriptionKey" value="secret value to be encrypted" />    
  </secureAppSettings>
  ...	
</configuration>
```
Note that the placeholders in `<appSettings>` are essential, but can have any value.


Also note that if you need to use another configuration builder for a developer workstation environment, such as Microsofts UserSecretsConfigBuilder there are additional considerations.
1) you will still need an empty `<secureAppSettings>` section
2) you will need to add the `UserSecretsConfigBuilder` in `web.config`

```xml
<add name="Secrets" userSecretsId="1ba9fc5a-d5e2-4dd3-8af2-1dfafd1b6db6" type="Microsoft.Configuration.ConfigurationBuilders.UserSecretsConfigBuilder, Microsoft.Configuration.ConfigurationBuilders.UserSecrets, Version=1.0.0.0, Culture=neutral" />
```
3) in your main web.config you should use `<appSettings configBuilders="Secrets">`
4) in all of your environment web config transforms you will need to switch the configuration builder definitions with transforms like these:
```xml
 <configBuilders>
    <builders>
      <!-- remove the user secrets configuration builder completely -->
      <add name="Secrets" value="" type="" xdt:Transform="Remove" xdt:Locator="Match(name)" />

      <!-- add the secure app settings configuration builder -->
      <add name="SecureAppSettings" type="Veracity.Authentication.OpenIDConnect.AspNet.SecureAppSettingsConfigurationBuilder, Veracity.Authentication.OpenIDConnect.AspNet, Version=1.0.0.0, Culture=neutral" xdt:Transform="Insert" xdt:Locator="Match(name)" />
    </builders>
  </configBuilders>

<appSettings configBuilders="SecureAppSettings" xdt:Transform="SetAttributes(configBuilders)">
...
</appSettings>
```
