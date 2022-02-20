using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Recaptcha.Verify.Net.Models;
using Refit;
using System;

namespace Recaptcha.Verify.Net.Extensions
{
    /// <summary>
    /// Extensions for configuring <see cref="RecaptchaService"/> for usage with dependency injection.
    /// </summary>
    public static class ConfigurationExtensions
    {
        private const string _baseUrl = "https://www.google.com/recaptcha/api";

        /// <summary>
        /// Configures <see cref="RecaptchaService"/> for usage with dependency injection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services.</param>
        /// <param name="recaptchaOptions">Configuration section for mapping to <see cref="RecaptchaOptions"/>.</param>
        /// <param name="configuration">Delegate for configuring options <see cref="RecaptchaOptions"/>.</param>
        public static void ConfigureRecaptcha(this IServiceCollection services, IConfigurationSection recaptchaOptions,
            Action<RecaptchaOptions> configuration = null)
        {
            services.Configure<RecaptchaOptions>(options =>
            {
                recaptchaOptions.Bind(options);
                recaptchaOptions.GetSection("ActionsScoreThresholds").Bind(options.ActionsScoreThresholds);
                configuration?.Invoke(options);
            });

            services.ConfigureService();
        }

        /// <summary>
        /// Configures <see cref="RecaptchaService"/> for usage with dependency injection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services.</param>
        /// <param name="recaptchaOptions">Recaptcha service options.</param>
        /// <param name="configuration">Delegate for configuring options <see cref="RecaptchaOptions"/>.</param>
        public static void ConfigureRecaptcha(this IServiceCollection services, RecaptchaOptions recaptchaOptions,
            Action<RecaptchaOptions> configuration = null)
        {
            configuration?.Invoke(recaptchaOptions);

            services.Configure<RecaptchaOptions>(co =>
            {
                co.SecretKey = recaptchaOptions.SecretKey;
                co.ScoreThreshold = recaptchaOptions.ScoreThreshold;
                co.ActionsScoreThresholds = recaptchaOptions.ActionsScoreThresholds;
            });

            services.ConfigureService();
        }

        /// <summary>
        /// Configures <see cref="RecaptchaService"/> for usage with dependency injection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services.</param>
        /// <param name="configuration">Delegate for configuring options <see cref="RecaptchaOptions"/>.</param>
        public static void ConfigureRecaptcha(this IServiceCollection services, Action<RecaptchaOptions> configuration = null) =>
            services.ConfigureRecaptcha(new RecaptchaOptions(), configuration);

        private static void ConfigureService(this IServiceCollection services)
        {
            services
                .AddRefitClient<IRecaptchaClient>()
                .ConfigureHttpClient(provider => provider.BaseAddress = new Uri(_baseUrl));

            services.AddSingleton<IRecaptchaService, RecaptchaService>();
        }
    }
}
