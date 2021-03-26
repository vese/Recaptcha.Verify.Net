using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Recaptcha.Verify.Net.Models;
using Refit;
using System;

namespace Recaptcha.Verify.Net.Extensions
{
    public static class Configuration
    {
        public static void ConfigureRecaptcha(this IServiceCollection services, IConfigurationSection recaptchaOptions)
        {
            services.Configure<RecaptchaOptions>(recaptchaOptions);

            services
                .AddRefitClient<IRecaptchaClient>(provider => new RefitSettings(new NewtonsoftJsonContentSerializer()))
                .ConfigureHttpClient(provider => provider.BaseAddress = new Uri("https://www.google.com/recaptcha/api"));

            services.AddSingleton<IRecaptchaService, RecaptchaService>();
        }
    }
}
