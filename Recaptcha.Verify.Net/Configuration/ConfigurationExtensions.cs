using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using System;

namespace Recaptcha.Verify.Net.Configuration
{
    /// <summary>
    /// Extensions for configuring <see cref="RecaptchaService"/> for usage with dependency injection.
    /// </summary>
    public static class ConfigurationExtensions
    {
        private const string _baseUrl = "https://www.google.com/recaptcha/api";

        /// <summary>
        /// Registers <see cref="IRecaptchaService"/> for usage with dependency injection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services.</param>
        /// <param name="recaptchaOptions">Configuration section for mapping to <see cref="RecaptchaOptions"/>.</param>
        /// <param name="configuration">Delegate for configuring options <see cref="RecaptchaOptions"/>.</param>
        public static IServiceCollection AddRecaptcha(this IServiceCollection services, IConfigurationSection recaptchaOptions, Action<RecaptchaOptions> configuration = null)
        {
            services.Configure<RecaptchaOptions>(options =>
            {
                recaptchaOptions.Bind(options);
                recaptchaOptions.GetSection(nameof(RecaptchaOptions.ActionsScoreThresholds)).Bind(options.ActionsScoreThresholds);
                configuration?.Invoke(options);
            });

            services.ConfigureService();

            return services;
        }

        /// <summary>
        /// Registers <see cref="IRecaptchaService"/> for usage with dependency injection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services.</param>
        /// <param name="recaptchaOptions">Recaptcha service options.</param>
        /// <param name="configuration">Delegate for configuring options <see cref="RecaptchaOptions"/>.</param>
        public static IServiceCollection AddRecaptcha(this IServiceCollection services, RecaptchaOptions recaptchaOptions, Action<RecaptchaOptions> configuration = null)
        {
            configuration?.Invoke(recaptchaOptions);
            services.AddSingleton(Options.Create(recaptchaOptions));
            services.ConfigureService();

            return services;
        }

        /// <summary>
        /// Registers <see cref="IRecaptchaService"/> for usage with dependency injection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services.</param>
        /// <param name="configuration">Delegate for configuring options <see cref="RecaptchaOptions"/>.</param>
        public static IServiceCollection AddRecaptcha(this IServiceCollection services, Action<RecaptchaOptions> configuration = null) =>
            services.AddRecaptcha(new RecaptchaOptions(), configuration);

        private static void ConfigureService(this IServiceCollection services)
        {
            services
                .AddRefitClient<IRecaptchaClient>()
                .ConfigureHttpClient(provider => provider.BaseAddress = new Uri(_baseUrl));

            services.AddScoped<IRecaptchaService, RecaptchaService>();
        }
    }
}
