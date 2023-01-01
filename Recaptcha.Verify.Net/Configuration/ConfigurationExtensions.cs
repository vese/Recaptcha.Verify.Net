using Microsoft.Extensions.DependencyInjection;
using System;

namespace Recaptcha.Verify.Net.Configuration
{
    /// <summary>
    /// Extensions for configuring <see cref="RecaptchaService"/> for usage with dependency injection.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Configures <see cref="RecaptchaService"/> for usage with dependency injection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services.</param>
        /// <param name="configuration">Delegate for configuring builder <see cref="RecaptchaConfigurationBuilder"/>.</param>
        public static IServiceCollection AddRecaptcha(this IServiceCollection services, Action<RecaptchaConfigurationBuilder> configuration)
        {
            var builder = new RecaptchaConfigurationBuilder();
            configuration.Invoke(builder);
            builder.Build(services);
            return services;
        }
    }
}
