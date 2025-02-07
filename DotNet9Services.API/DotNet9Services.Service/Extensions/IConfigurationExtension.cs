using Microsoft.Extensions.Configuration;

namespace DotNet9Services.Service.Extensions
{
    /// <summary>
    /// Extension class for <see cref="IConfiguration"/>
    /// </summary>
    public static class IConfigurationExtension
    {
        /// <summary>
        /// Throws an exception if the value associated with the specified key in the configuration is null or empty.
        /// </summary>
        /// <param name="config">The IConfiguration instance.</param>
        /// <param name="key">The key to retrieve the value from the configuration.</param>
        /// <returns>The value associated with the specified key.</returns>
        public static string B3ThrowIfNullOrEmpty(this IConfiguration config, string key)
        {
            string? value = config[key];
            return string.IsNullOrEmpty(value) ? throw new Exception($"{key} is null or empty in appsettings/Keyvault") : value;
        }

        /// <summary>
        /// Returns the default value if the value associated with the specified key in the configuration is null or empty.
        /// </summary>
        /// <param name="config">The IConfiguration instance.</param>
        /// <param name="key">The key to retrieve the value from the configuration.</param>
        /// <param name="defaultValue">The default value to return if the value associated with the key is null or empty.</param>
        /// <returns>The value associated with the specified key, or the default value if the value is null or empty.</returns>
        public static string B3DefaultIfNullOrEmpty(this IConfiguration config, string key, string defaultValue)
        {
            string? value = config[key];
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }
    }
}
