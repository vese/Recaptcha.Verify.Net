using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Recaptcha.Verify.Net.Client;
using Recaptcha.Verify.Net.Service;
using Recaptcha.Verify.Net.TokenExtraction;
using Recaptcha.Verify.Net.TokenVerification;

namespace Recaptcha.Verify.Net.Configuration;

/// <summary>
/// Extensions for configuring services for usage with dependency injection.
/// </summary>
public static class ConfigurationExtensions
{
    private const string _baseUrl = "https://www.google.com/recaptcha/api";

    /// <summary>
    /// Registers <see cref="IRecaptchaVerificationResultValidationService"/> implementation for usage with dependency injection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services.</param>
    /// <param name="configuration">Delegate for configuring options <see cref="RecaptchaOptions"/>.</param>
    public static IServiceCollection AddRecaptcha(this IServiceCollection services, Action<RecaptchaOptions>? configuration = null) =>
        services.AddRecaptcha(new RecaptchaOptions(), configuration);

    /// <summary>
    /// Registers <see cref="IRecaptchaVerificationResultValidationService"/> implementation for usage with dependency injection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services.</param>
    /// <param name="section">Configuration section for mapping to <see cref="RecaptchaOptions"/>.</param>
    /// <param name="configuration">Delegate for configuring options <see cref="RecaptchaOptions"/>.</param>
    public static IServiceCollection AddRecaptcha(this IServiceCollection services, IConfigurationSection section, Action<RecaptchaOptions>? configuration = null)
    {
        var recaptchaOptions = new RecaptchaOptions();
        section.Bind(recaptchaOptions);
        section.GetSection(nameof(RecaptchaOptions.ActionsScoreThresholds)).Bind(recaptchaOptions.ActionsScoreThresholds);

        return services.AddRecaptcha(recaptchaOptions, configuration);
    }

    /// <summary>
    /// Registers <see cref="IRecaptchaVerificationResultValidationService"/> implementation for usage with dependency injection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services.</param>
    /// <param name="recaptchaOptions">Recaptcha service options.</param>
    /// <param name="configuration">Delegate for configuring options <see cref="RecaptchaOptions"/>.</param>
    public static IServiceCollection AddRecaptcha(this IServiceCollection services, RecaptchaOptions recaptchaOptions, Action<RecaptchaOptions>? configuration = null)
    {
        configuration?.Invoke(recaptchaOptions);
        services.AddSingleton(Options.Create(recaptchaOptions));

        services.AddTokenExtractorForOptions(recaptchaOptions);

        services.ConfigureService();

        return services;
    }

    private static void AddTokenExtractorForOptions(this IServiceCollection services, RecaptchaOptions options)
    {
        if (!string.IsNullOrEmpty(options.AttributeOptions.ResponseTokenNameInHeader))
        {
            services.AddRecaptchaHeaderTokenExtractor(options.AttributeOptions.ResponseTokenNameInHeader);
        }

        if (!string.IsNullOrEmpty(options.AttributeOptions.ResponseTokenNameInQuery))
        {
            services.AddRecaptchaQueryTokenExtractor(options.AttributeOptions.ResponseTokenNameInQuery);
        }

        if (!string.IsNullOrEmpty(options.AttributeOptions.ResponseTokenNameInForm))
        {
            services.AddRecaptchaFormTokenExtractor(options.AttributeOptions.ResponseTokenNameInForm);
        }

        if (options.AttributeOptions.GetResponseTokenFromActionArguments is not null)
        {
            services.AddRecaptchaActionArgumentsTokenExtractor(options.AttributeOptions.GetResponseTokenFromActionArguments);
        }

        if (options.AttributeOptions.GetResponseTokenFromExecutingContext is not null)
        {
            services.AddRecaptchaExecutingContextTokenExtractor(options.AttributeOptions.GetResponseTokenFromExecutingContext);
        }
    }

    /// <summary>
    /// Registers <see cref="IRecaptchaTokenExtractor"/> implementation for usage with dependency injection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services.</param>
    /// <param name="getToken">
    /// Delegate for getting reCAPTCHA response token from action arguments.
    /// Actions argumets are mapped arguments of controller method.
    /// </param>
    public static IServiceCollection AddRecaptchaActionArgumentsTokenExtractor(this IServiceCollection services, Func<IDictionary<string, object?>, string?> getToken) =>
        services.AddSingleton<IRecaptchaTokenExtractor, ActionArgumentsTokenExtractor>(_ => new ActionArgumentsTokenExtractor(getToken));

    /// <summary>
    /// Registers <see cref="IRecaptchaTokenExtractor"/> implementation for usage with dependency injection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services.</param>
    /// <param name="argumentName">
    /// Name of action argument that contains reCAPTCHA token.
    /// Actions argumets are mapped arguments of controller method.
    /// </param>
    public static IServiceCollection AddRecaptchaActionArgumentsTokenExtractor(this IServiceCollection services, string argumentName) =>
        services.AddSingleton<IRecaptchaTokenExtractor, ActionArgumentsTokenExtractor>(_ => new ActionArgumentsTokenExtractor(argumentName));

    /// <summary>
    /// Registers <see cref="IRecaptchaTokenExtractor"/> implementation for usage with dependency injection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services.</param>
    /// <param name="getToken">Delegate for getting reCAPTCHA response token from executing context.</param>
    public static IServiceCollection AddRecaptchaExecutingContextTokenExtractor(this IServiceCollection services, Func<ActionExecutingContext, string?> getToken) =>
        services.AddSingleton<IRecaptchaTokenExtractor, ExecutingContextTokenExtractor>(_ => new ExecutingContextTokenExtractor(getToken));

    /// <summary>
    /// Registers <see cref="IRecaptchaTokenExtractor"/> implementation for usage with dependency injection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services.</param>
    /// <param name="parameterName">Name of form that contains reCAPTCHA token.</param>
    public static IServiceCollection AddRecaptchaFormTokenExtractor(this IServiceCollection services, string parameterName) =>
        services.AddSingleton<IRecaptchaTokenExtractor, FormTokenExtractor>(_ => new FormTokenExtractor(parameterName));

    /// <summary>
    /// Registers <see cref="IRecaptchaTokenExtractor"/> implementation for usage with dependency injection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services.</param>
    /// <param name="headerName">Name of header that contains reCAPTCHA token.</param>
    public static IServiceCollection AddRecaptchaHeaderTokenExtractor(this IServiceCollection services, string headerName) =>
        services.AddSingleton<IRecaptchaTokenExtractor, HeaderTokenExtractor>(_ => new HeaderTokenExtractor(headerName));

    /// <summary>
    /// Registers <see cref="IRecaptchaTokenExtractor"/> implementation for usage with dependency injection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services.</param>
    /// <param name="parameterName">Name of query parameter that contains reCAPTCHA token.</param>
    [Obsolete("Do not pass token in query parameters. It is not secure.")]
    public static IServiceCollection AddRecaptchaQueryTokenExtractor(this IServiceCollection services, string parameterName) =>
        services.AddSingleton<IRecaptchaTokenExtractor, QueryTokenExtractor>(_ => new QueryTokenExtractor(parameterName));

    private static void ConfigureService(this IServiceCollection services)
    {
        services.AddHttpClient<IRecaptchaClient, RecaptchaClient>(client =>
            client.BaseAddress = new Uri(_baseUrl.EndsWith('/') ? _baseUrl : $"{_baseUrl}/"));

        services.AddScoped<IRecaptchaVerificationService, RecaptchaVerificationService>();
        services.AddScoped<IRecaptchaVerificationResultValidationService, RecaptchaVerificationResultValidationService>();
    }
}
