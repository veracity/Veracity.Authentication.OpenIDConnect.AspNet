// <copyright file="MSALAppMemoryTokenCache.cs" company="DNV GL Veracity">
//  Licensed under the MIT License.   
// </copyright>
// <summary>
//   An implementation of token cache for Confidential client applications backed by MemoryCache
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Veracity.Authentication.OpenIDConnect.AspNet
{
    using System;
    using System.Runtime.Caching;
    using Microsoft.Identity.Client;

    /// <summary>
    /// An implementation of token cache for Confidential client applications backed by MemoryCache.
    /// MemoryCache is useful in Api scenarios where there is no HttpContext to cache data.
    /// </summary>
    /// <seealso cref="https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization"/>
    public class MSALAppMemoryTokenCache
    {
        /// <summary>
        /// The application cache key
        /// </summary>
        private readonly string appCacheId;

        /// <summary>
        /// The backing MemoryCache instance
        /// </summary>
        private readonly MemoryCache memoryCache = MemoryCache.Default;

		/// <summary>
		/// The duration the tokens are kept in memory cache. In production, a higher value up to 90 days is recommended.
		/// The token cache will contain both AccessToken and RefreshToken, which they last 1h and 90 days, respectively, by default.
		/// </summary>
		private readonly DateTimeOffset cacheDuration = DateTimeOffset.Now.AddHours(48);

        /// <summary>
        /// The internal handle to the client's instance of the Cache
        /// </summary>
        private readonly ITokenCache appTokenCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSALAppMemoryTokenCache"/> class.
        /// </summary>
        /// <param name="tokenCache">The client's instance of the token cache.</param>
        /// <param name="clientId">The application's id (Client ID).</param>
        public MSALAppMemoryTokenCache(ITokenCache tokenCache, string clientId)
        {
            appCacheId = clientId + "_AppTokenCache";

            if (appTokenCache == null)
            {
                appTokenCache = tokenCache;
                appTokenCache.SetBeforeAccess(AppTokenCacheBeforeAccessNotification);
                appTokenCache.SetAfterAccess(AppTokenCacheAfterAccessNotification);
                appTokenCache.SetBeforeWrite(AppTokenCacheBeforeWriteNotification);
            }

            LoadAppTokenCacheFromMemory();
        }

        /// <summary>
        /// if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void AppTokenCacheBeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // Since we are using a MemoryCache ,whose methods are threads safe, we need not to do anything in this handler.
        }

        /// <summary>
        /// Loads the application's token from memory cache.
        /// </summary>
        private void LoadAppTokenCacheFromMemory()
        {
            // Ideally, methods that load and persist should be thread safe. MemoryCache.Get() is thread safe.
            byte[] tokenCacheBytes = (byte[])memoryCache.Get(appCacheId);
            appTokenCache.DeserializeMsalV3(tokenCacheBytes);
        }

        /// <summary>
        /// Persists the application's token to the cache.
        /// </summary>
        private void PersistAppTokenCache()
        {
            // Ideally, methods that load and persist should be thread safe.MemoryCache.Get() is thread safe.
            // Reflect changes in the persistence store
            memoryCache.Set(appCacheId, appTokenCache.SerializeMsalV3(), cacheDuration);
        }

        public void Clear()
        {
            memoryCache.Remove(appCacheId);

            // Nulls the currently deserialized instance
            LoadAppTokenCacheFromMemory();
        }

        /// <summary>
        /// Triggered right before MSAL needs to access the cache. Reload the cache from the persistence store in case it changed since the last access.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void AppTokenCacheBeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            LoadAppTokenCacheFromMemory();
        }

        /// <summary>
        /// Triggered right after MSAL accessed the cache.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private void AppTokenCacheAfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                PersistAppTokenCache();
            }
        }
    }
}