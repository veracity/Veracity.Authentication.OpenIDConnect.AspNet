// <copyright file="ClaimPrincipalExtension.cs" company="DNV GL Veracity">
//  Licensed under the MIT License.   
// </copyright>
// <summary>
//   Extension methods for dealing with claims principals
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Veracity.Authentication.OpenIDConnect.AspNet
{
    using System.Security.Claims;
    using Microsoft.Identity.Client;

    /// <summary>
    /// Extension methods for dealing with claims principals
    /// </summary>
    public static class ClaimsPrincipalExtension
    {
        /// <summary>
        /// Get the Account identifier for an MSAL.NET account from a ClaimsPrincipal
        /// </summary>
        /// <param name="claimsPrincipal">Claims principal</param>
        /// <returns>A string corresponding to an account identifier as defined in <see cref="Microsoft.Identity.Client.AccountId.Identifier"/></returns>
        public static string GetMsalAccountId(this ClaimsPrincipal claimsPrincipal)
        {
            var accountIdClaim = claimsPrincipal.FindFirst(ClaimConstants.MsalAccountId);

            if (accountIdClaim == null)
            {
                string userObjectId = GetObjectId(claimsPrincipal);
                string tenantId = GetTenantId(claimsPrincipal);

                if (!string.IsNullOrWhiteSpace(userObjectId) && !string.IsNullOrWhiteSpace(tenantId))
                {
                    return $"{userObjectId}.{tenantId}";
                }

                return null;
            }

            return accountIdClaim.Value;
        }


        /// <summary>
        /// Gets the MsalAccountId to use for the supplied IAccount.
        /// </summary>
        /// <param name="account">The identity client account.</param>
        /// <returns></returns>
        public static Claim ToMsalAccountId(this IAccount account)
        {
            var result = new Claim(ClaimConstants.MsalAccountId, account.HomeAccountId.Identifier);
            return result;
        }

        /// <summary>
        /// Get the unique object ID associated with the claimsPrincipal
        /// </summary>
        /// <param name="claimsPrincipal">Claims principal from which to retrieve the unique object id</param>
        /// <returns>Unique object ID of the identity, or <c>null</c> if it cannot be found</returns>
        public static string GetObjectId(this ClaimsPrincipal claimsPrincipal)
        {
            var objIdClaim = claimsPrincipal.FindFirst(ClaimConstants.ObjectId)
                             ?? claimsPrincipal.FindFirst("oid");

            return objIdClaim != null
                ? objIdClaim.Value
                : string.Empty;
        }

        /// <summary>
        /// Tenant ID of the identity
        /// </summary>
        /// <param name="claimsPrincipal">Claims principal from which to retrieve the tenant id</param>
        /// <returns>Tenant ID of the identity, or <c>null</c> if it cannot be found</returns>
        public static string GetTenantId(this ClaimsPrincipal claimsPrincipal)
        {
            var tenantIdClaim = claimsPrincipal.FindFirst(ClaimConstants.TenantId)
                                ?? claimsPrincipal.FindFirst("tid");

            return tenantIdClaim != null
                ? tenantIdClaim.Value
                : string.Empty;
        }
    }
}