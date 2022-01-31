using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Recaptcha.Verify.Net.Models;
using Refit;
using System;

namespace Recaptcha.Verify.Net.Extensions
{
    public static class Configuration
    {
        private const string _baseUrl = "https://www.google.com/recaptcha/api";

        public static void ConfigureRecaptcha(this IServiceCollection services, IConfigurationSection recaptchaOptions)
        {
            services.Configure<RecaptchaOptions>(settings =>
            {
                recaptchaOptions.Bind(settings);
                recaptchaOptions.GetSection("ActionsScoreThresholds").Bind(settings.ActionsScoreThresholds);
            });

            services.ConfigureService();
        }

        public static void ConfigureRecaptcha(this IServiceCollection services, RecaptchaOptions recaptchaOptions)
        {
            services.Configure<RecaptchaOptions>(co =>
            {
                co.SecretKey = recaptchaOptions.SecretKey;
                co.ScoreThreshold = recaptchaOptions.ScoreThreshold;
                co.ActionsScoreThresholds = recaptchaOptions.ActionsScoreThresholds;
            });

            services.ConfigureService();
        }

        public static void ConfigureRecaptcha(this IServiceCollection services) => services.ConfigureService();

        private static void ConfigureService(this IServiceCollection services)
        {
            services
                .AddRefitClient<IRecaptchaClient>(provider => new RefitSettings(new NewtonsoftJsonContentSerializer()))
                .ConfigureHttpClient(provider => provider.BaseAddress = new Uri(_baseUrl));

            services.AddSingleton<IRecaptchaService, RecaptchaService>();
        }
    }
}
