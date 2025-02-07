using Azure.Identity;
using Azure;
using Microsoft.AspNetCore.Builder;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.Extensions.Configuration;

namespace DotNet9Services.Service.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="WebApplicationBuilder"/>
    /// </summary>
    public static class BuilderExtension
    {
        /// <summary>
        /// Configures the application to use Azure Key Vault for configuration settings.
        /// </summary>
        /// <param name="builder">The <see cref="WebApplicationBuilder"/> instance to configure Key Vault for.</param>
        /// <remarks>
        /// This method integrates Azure Key Vault into the application's configuration system. It retrieves the Key Vault
        /// endpoint from the application's configuration, creates a <see cref="SecretClient"/> using the endpoint and the 
        /// default Azure credentials, and adds the Key Vault as a configuration source with a reload interval.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the Key Vault endpoint is null or empty.
        /// </exception>
        public static void B3KeyVault(this WebApplicationBuilder builder)
        {

            Uri keyVaultEndpoint = builder.Configuration.B3ThrowIfNullOrEmpty("Kv").ToUri();
            DefaultAzureCredential credential = new();
            AzureKeyVaultConfigurationOptions options = new()
            {
                ReloadInterval = TimeSpan.FromHours(24),
            };

            try
            {
                builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, credential, options);
            }
            catch (AggregateException ex)
            {
                //Can't find the keyvault.
                Console.WriteLine("\r\n");
                Console.WriteLine($"Can't find keyvault: {keyVaultEndpoint}\r\n{ex.Message}");
                Console.WriteLine("\r\n");
                throw;
            }
            catch (RequestFailedException ex)
            {
                //The user is unauthorized.
                Console.WriteLine("\r\n");
                Console.WriteLine($"Unauthorized to connect to keyvault: {keyVaultEndpoint}\r\n{ex.Message}");
                Console.WriteLine("\r\n");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception of type {ex.GetType().Name} was thrown.\r\n{ex.Message}");
                throw;
            }
        }
    }
}
