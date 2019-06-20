// <copyright file="SecureAppSettingsConfigurationBuilder.cs" company="DNV GL Veracity">
//  Licensed under the MIT License.   
// </copyright>
// <summary>
//   Class to allow secure app settings to 'appear' as normal AppSettings
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Veracity.Authentication.OpenIDConnect.AspNet
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Linq;
    using Microsoft.Configuration.ConfigurationBuilders;

    /// <summary>
    /// Class to allow secure app settings to 'appear' as normal AppSettings - to allow the Veracity package
    /// to transparently read configuration values that are encrypted
    /// </summary>
    /// <seealso cref="KeyValueConfigBuilder" />
    public class SecureAppSettingsConfigurationBuilder : KeyValueConfigBuilder
    {
        private const string SecureApplicationSettingsSection = "secureAppSettings";

        /// <summary>
        /// Looks up a single 'value' for the given 'key.'
        /// </summary>
        /// <param name="key">The 'key' to look up in the config source. (Prefix handling is not needed here.)</param>
        /// <returns>
        /// The value corresponding to the given 'key' or null if no value is found.
        /// </returns>
        /// <exception cref="Exception">Configuration section '{SecureApplicationSettingsSection}</exception>
        public override string GetValue(string key)
        {
            var secureApplicationSettings = ConfigurationManager.GetSection(SecureApplicationSettingsSection);

            if (secureApplicationSettings == null)
            {
                throw new Exception($"Configuration section '{SecureApplicationSettingsSection}' not found.");
            }

            if (secureApplicationSettings is NameValueCollection nameValueCollection && nameValueCollection.AllKeys.Contains(key))
            {
                return nameValueCollection[key];
            }

            return null;
        }

        /// <summary>
        /// Retrieves all known key/value pairs for the configuration source where the key begins with with <paramref name="prefix" />.
        /// </summary>
        /// <param name="prefix">A prefix string to filter the list of potential keys retrieved from the source.</param>
        /// <returns>
        /// A collection of key/value pairs.
        /// </returns>
        /// <exception cref="Exception">Configuration section '{SecureApplicationSettingsSection}</exception>
        public override ICollection<KeyValuePair<string, string>> GetAllValues(string prefix)
        {
            object secureApplicationSettings = ConfigurationManager.GetSection(SecureApplicationSettingsSection);

            if (secureApplicationSettings == null)
            {
                throw new Exception($"Configuration section '{SecureApplicationSettingsSection}' not found.");
            }

            if (secureApplicationSettings is NameValueCollection nameValueCollection)
            {
                var dictionary = nameValueCollection.AllKeys.ToDictionary(t => t, t => nameValueCollection[t]);

                return dictionary;
            }

            return null;
        }
    }
}