// <copyright file="Constants.cs" company="DNV GL Veracity">
// Licensed under the MIT License.   
// </copyright>
// <summary>
//   Defines the Identity Claims.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Veracity.Authentication.OpenIDConnect.AspNet
{
    /// <summary>
    /// Claim key constants
    /// </summary>
    public static class ClaimConstants
    {
        public const string ObjectId = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        public const string TenantId = "http://schemas.microsoft.com/identity/claims/tenantid";
        public const string MsalAccountId = "msal-account-id";
    }
}