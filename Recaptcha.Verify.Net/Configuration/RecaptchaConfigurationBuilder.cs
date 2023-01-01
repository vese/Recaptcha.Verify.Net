using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Recaptcha.Verify.Net.Logging;
using Refit;
using System;

namespace Recaptcha.Verify.Net.Configuration
{
    public class RecaptchaConfigurationBuilder
    {
        private const string _baseUrl = "https://www.google.com/recaptcha/api";

        private IConfigurationSection _recaptchaOptionsConfiguration;
        private Action<RecaptchaOptions> _recaptchaOptionsConfigurationAction;
        private RecaptchaOptions _recaptchaOptions;
        private Action<LogOptions> _logOptionsConfigurationAction;
        private LogOptions _logOptions;
        private bool _builded = false;

        #region Configure properties
        public RecaptchaConfigurationBuilder Configure(IConfigurationSection recaptchaOptions, Action<RecaptchaOptions> configuration = null)
        {
            _recaptchaOptionsConfiguration = recaptchaOptions;
            _recaptchaOptionsConfigurationAction = configuration;
            return this;
        }
        public RecaptchaConfigurationBuilder Configure(RecaptchaOptions recaptchaOptions, Action<RecaptchaOptions> configuration = null)
        {
            _recaptchaOptions = recaptchaOptions;
            _recaptchaOptionsConfigurationAction = configuration;
            return this;
        }
        public RecaptchaConfigurationBuilder Configure(Action<RecaptchaOptions> configuration)
        {
            _recaptchaOptionsConfigurationAction = configuration;
            return this;
        }
        public RecaptchaConfigurationBuilder ConfigureLogging(LogOptions logOptions, Action<LogOptions> configuration = null)
        {
            _logOptions = logOptions;
            _logOptionsConfigurationAction = configuration;
            return this;
        }
        public RecaptchaConfigurationBuilder ConfigureLogging(Action<LogOptions> configuration)
        {
            _logOptionsConfigurationAction = configuration;
            return this;
        }
        #endregion

        #region Build
        public void Build(IServiceCollection services)
        {
            if (_builded)
            {
                return;
            }

            if (_recaptchaOptionsConfiguration != null)
            {
                services.Configure<RecaptchaOptions>(options =>
                {
                    _recaptchaOptionsConfiguration.Bind(options);
                    _recaptchaOptionsConfigurationAction?.Invoke(options);
                    ConfigureLogOptions(services, options);
                });
            }
            else
            {
                var options = _recaptchaOptions ?? new RecaptchaOptions();
                _recaptchaOptionsConfigurationAction?.Invoke(options);
                ConfigureLogOptions(services, options);
                services.AddSingleton(Options.Create(options));
            }

            ConfigureServices(services);
        }

        private void ConfigureLogOptions(IServiceCollection services, RecaptchaOptions options)
        {
            if (_logOptions != null)
            {
                options.LogOptions = _logOptions;
            }

            if (options.LogOptions == null)
            {
                options.LogOptions = new LogOptions();
            }

            _logOptionsConfigurationAction?.Invoke(options.LogOptions);

            services.AddSingleton(Options.Create(options.LogOptions));
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services
                .AddRefitClient<IRecaptchaClient>()
                .ConfigureHttpClient(provider => provider.BaseAddress = new Uri(_baseUrl));

            services.AddScoped<IRecaptchaService, RecaptchaService>();
            services.AddScoped<IRecaptchaLoggerService, RecaptchaLoggerService>();
        }
        #endregion
    }
}
