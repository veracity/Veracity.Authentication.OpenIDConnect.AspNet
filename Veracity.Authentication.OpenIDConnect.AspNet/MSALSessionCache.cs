// <copyright file="MSALSessionCache.cs" company="DNV GL Veracity">
//  Licensed under the Apache License, Version 2.0.   
// </copyright>
// <summary>
//   Defines the MSALSessionCache type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Veracity.Authentication.OpenIDConnect.AspNet
{
    using System.Threading;
    using System.Web;

    using Microsoft.Identity.Client;

    internal class MSALSessionCache
    {
        private static ReaderWriterLockSlim SessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        string UserId = string.Empty;
        string CacheId = string.Empty;
        HttpContextBase httpContext = null;

        TokenCache cache = new TokenCache();

        public MSALSessionCache(string userId, HttpContextBase httpcontext)
        {
            this.UserId = userId;
            this.CacheId = this.UserId + "_TokenCache";
            this.httpContext = httpcontext;
            this.Load();
        }

        public TokenCache GetMsalCacheInstance()
        {
            this.cache.SetBeforeAccess(this.BeforeAccessNotification);
            this.cache.SetAfterAccess(this.AfterAccessNotification);
            this.Load();
            return this.cache;
        }

        public void SaveUserStateValue(string state)
        {
            SessionLock.EnterWriteLock();
            this.httpContext.Session[this.CacheId + "_state"] = state;
            SessionLock.ExitWriteLock();
        }
        public string ReadUserStateValue()
        {
            string state = string.Empty;
            SessionLock.EnterReadLock();
            state = (string)this.httpContext.Session[this.CacheId + "_state"];
            SessionLock.ExitReadLock();
            return state;
        }
        public void Load()
        {
            SessionLock.EnterReadLock();
            this.cache.Deserialize((byte[])this.httpContext.Session[this.CacheId]);
            SessionLock.ExitReadLock();
        }

        public void Persist()
        {
            SessionLock.EnterWriteLock();

            // Optimistically set HasStateChanged to false. We need to do it early to avoid losing changes made by a concurrent thread.
            this.cache.HasStateChanged = false;

            // Reflect changes in the persistent store
            this.httpContext.Session[this.CacheId] = this.cache.Serialize();
            SessionLock.ExitWriteLock();
        }

        // Triggered right before MSAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            this.Load();
        }

        // Triggered right after MSAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (this.cache.HasStateChanged)
            {
                this.Persist();
            }
        }
    }
}