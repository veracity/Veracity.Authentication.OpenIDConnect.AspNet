// <copyright file="MSALPerUserMemoryTokenCache.cs" company="DNV GL Veracity">
//  Licensed under the MIT License.   
// </copyright>
// <summary>
//   An implementation of token cache for both Confidential and Public clients backed by MemoryCache
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Veracity.Authentication.OpenIDConnect.AspNet
{
    using System;
    using System.Runtime.Caching;
    using System.Security.Claims;
    using Microsoft.Identity.Client;

    /// <summary>
    /// An implementation of token cache for both Confidential and Public clients backed by MemoryCache.
    /// MemoryCache is useful in Api scenarios where there is no HttpContext to cache data.
    /// </summary>
    /// <seealso cref="https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization"/>
    public class MSALPerUserMemoryTokenCache
    {
        /// <summary>
        /// The backing MemoryCache instance
        /// </summary>
        private readonly MemoryCache memoryCache = MemoryCache.Default;

        /// <summary>
        /// The duration that the tokens are kept in memory cache.
        /// Veracity recommends that this is set to half of the refresh token lifetime of 7 days, hence 3.5 days.
        /// </summary>
        private readonly DateTimeOffset cacheDuration = DateTimeOffset.Now.AddDays(3.5);

        /// <summary>
        /// The internal handle to the client's instance of the Cache
        /// </summary>
        private ITokenCache userTokenCache;

        /// <summary>
        /// Once the user signs in, this will not be null and can be obtained via a call to Thread.CurrentPrincipal
        /// </summary>
        internal ClaimsPrincipal SignedInUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSALPerUserMemoryTokenCache"/> class.
        /// </summary>
        /// <param name="tokenCache">The client's instance of the token cache.</param>
        public MSALPerUserMemoryTokenCache(ITokenCache tokenCache)
        {
            Initialize(tokenCache, ClaimsPrincipal.Current);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MSALPerUserMemoryTokenCache"/> class.
        /// </summary>
        /// <param name="tokenCache">The client's instance of the token cache.</param>
        /// <param name="user">The signed-in user for whom the cache needs to be established.</param>
        public MSALPerUserMemoryTokenCache(ITokenCache tokenCache, ClaimsPrincipal user)
        {
            Initialize(tokenCache, user);
        }

        /// <summary>Initializes the cache instance</summary>
        /// <param name="tokenCache">The ITokenCache passed through the constructor</param>
        /// <param name="user">The signed-in user for whom the cache needs to be established..</param>
        private void Initialize(ITokenCache tokenCache, ClaimsPrincipal user)
        {
            SignedInUser = user;

            userTokenCache = tokenCache;
            userTokenCache.SetBeforeAccess(UserTokenCacheBeforeAccessNotification);
            userTokenCache.SetAfterAccess(UserTokenCacheAfterAccessNotification);
            userTokenCache.SetBeforeWrite(UserTokenCacheBeforeWriteNotification);
        }

        /// <summary>
        /// Explores the Claims of a signed-in user (if available) to populate the unique Id of this cache's instance.
        /// </summary>
        /// <returns>The signed in user's object.tenant Id , if available in the ClaimsPrincipal.Current instance</returns>
        internal string GetMsalAccountId()
        {
            return SignedInUser?.GetMsalAccountId();
        }

        /// <summary>
        /// Loads the user token cache from memory.
        /// </summary>
        private void LoadUserTokenCacheFromMemory(ITokenCacheSerializer tokenCacheSerializer)
        {
            string cacheKey = GetMsalAccountId();

            if (string.IsNullOrWhiteSpace(cacheKey))
                return;

            // Ideally, methods that load and persist should be thread safe. MemoryCache.Get() is thread safe.
            byte[] tokenCacheBytes = (byte[])memoryCache.Get(GetMsalAccountId());

            tokenCacheSerializer.DeserializeMsalV3(tokenCacheBytes);
        }

        /// <summary>
        /// Persists the user token blob to the memoryCache.
        /// </summary>
        /// <param name="argsTokenCache"></param>
        private void PersistUserTokenCache(ITokenCacheSerializer argsTokenCache)
        {
            string cacheKey = GetMsalAccountId();

            if (string.IsNullOrWhiteSpace(cacheKey))
                return;

            // Ideally, methods that load and persist should be thread safe.MemoryCache.Get() is thread safe.
            var serialised = argsTokenCache.SerializeMsalV3();

            memoryCache.Set(cacheKey, serialised, cacheDuration);
        }

        /// <summary>
        /// Clears the TokenCache's copy of this user's cache.
        /// </summary>
        public void Clear()
        {
            memoryCache.Remove(GetMsalAccountId());
        }

        /// <summary>
        /// Triggered right after MSAL accessed the cache.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheAfterAccessNotification(TokenCacheNotificationArgs args)
        {
            SetSignedInUserFromNotificationArgs(args);

            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                PersistUserTokenCache(args.TokenCache);
            }
        }

        /// <summary>
        /// Triggered right before MSAL needs to access the cache. Reload the cache from the persistence store in case it changed since the last access.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheBeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            LoadUserTokenCacheFromMemory(args.TokenCache);
        }

        /// <summary>
        /// if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void UserTokenCacheBeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // Since we are using a MemoryCache ,whose methods are threads safe, we need not to do anything in this handler.
        }

        /// <summary>
        /// To keep the cache, ClaimsPrincipal and MemoryCache in sync, we ensure that the user's object Id we obtained by MSAL after
        /// successful sign-in is set as the key for the cache.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void SetSignedInUserFromNotificationArgs(TokenCacheNotificationArgs args)
        {
            if (SignedInUser == null || args.Account == null)
            {
                return;
            }

            if (!(SignedInUser.Identity is ClaimsIdentity identity))
            {
                return;
            }

            var accountCacheKeyClaim = args.Account.ToMsalAccountId();

            identity.AddClaim(accountCacheKeyClaim);
        }
    }
}