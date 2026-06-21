using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace Recaptcha.Verify.Net.Configuration;

/// <summary>
/// Extensions for configuring services for usage with dependency injection.
/// </summary>
public static class ConfigurationExtensions
{
    private const string DefaultBaseUrl = "https://www.google.com/recaptcha/api";

    /// <summary>
    /// Registers <see cref="IRecaptchaVerificationResultValidationService"/> implementation for usage with dependency injection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services.</param>
    /// <param name="configuration">Delegate for configuring options <see cref="RecaptchaOptions"/>.</param>
    /// <param name="configureHttpClient">Optional delegate to further configure the <see cref="HttpClient"/> used for the siteverify call (e.g. default request headers); applied after the library defaults (<c>BaseAddress</c>, <c>Timeout</c>).</param>
    public static IServiceCollection AddRecaptcha(this IServiceCollection services, Action<RecaptchaOptions>? configuration = null, Action<HttpClient>? configureHttpClient = null) =>
        services.AddRecaptcha(new RecaptchaOptions(), configuration, configureHttpClient);

    /// <summary>
    /// Registers <see cref="IRecaptchaVerificationResultValidationService"/> implementation for usage with dependency injection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services.</param>
    /// <param name="section">Configuration section for mapping to <see cref="RecaptchaOptions"/>.</param>
    /// <param name="configuration">Delegate for configuring options <see cref="RecaptchaOptions"/>.</param>
    /// <param name="configureHttpClient">Optional delegate to further configure the <see cref="HttpClient"/> used for the siteverify call (e.g. default request headers); applied after the library defaults (<c>BaseAddress</c>, <c>Timeout</c>).</param>
    public static IServiceCollection AddRecaptcha(this IServiceCollection services, IConfigurationSection section, Action<RecaptchaOptions>? configuration = null, Action<HttpClient>? configureHttpClient = null)
    {
        var recaptchaOptions = new RecaptchaOptions();
        section.Bind(recaptchaOptions);

        return services.AddRecaptcha(recaptchaOptions, configuration, configureHttpClient);
    }

    /// <summary>
    /// Registers <see cref="IRecaptchaVerificationResultValidationService"/> implementation for usage with dependency injection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services.</param>
    /// <param name="recaptchaOptions">Recaptcha service options.</param>
    /// <param name="configuration">Delegate for configuring options <see cref="RecaptchaOptions"/>.</param>
    /// <param name="configureHttpClient">Optional delegate to further configure the <see cref="HttpClient"/> used for the siteverify call (e.g. default request headers); applied after the library defaults (<c>BaseAddress</c>, <c>Timeout</c>).</param>
    public static IServiceCollection AddRecaptcha(this IServiceCollection services, RecaptchaOptions recaptchaOptions, Action<RecaptchaOptions>? configuration = null, Action<HttpClient>? configureHttpClient = null)
    {
        configuration?.Invoke(recaptchaOptions);

        services.ApplyLegacyAttributeOptions(recaptchaOptions);

        services.AddOptionsForServices(recaptchaOptions);

        services.AddTokenExtractorForOptions(recaptchaOptions);

        services.ConfigureService(recaptchaOptions.Verification.BaseUrl, recaptchaOptions.Verification.Timeout, configureHttpClient);

        return services;
    }

    private static void AddOptionsForServices(this IServiceCollection services, RecaptchaOptions options)
    {
        services.AddSingleton(Options.Create(options.Verification));
        services.AddSingleton(Options.Create(options.Validation));
        services.AddSingleton(Options.Create(options.Attribute));
    }

    /// <summary>
    /// Propagates the legacy <see cref="RecaptchaLegacyAttributeOptions"/> values (otherwise read nowhere)
    /// onto <see cref="RecaptchaOptions.Attribute"/> so legacy appsettings keep working.
    /// </summary>
    private static void ApplyLegacyAttributeOptions(this IServiceCollection services, RecaptchaOptions options)
    {
#pragma warning disable CS0618 // Suppress obsolete warnings for legacy backward-compatibility propagation
        var legacy = options.AttributeOptions;

        // OnVerificationFailed: the new (canonical) delegate wins; legacy fills the gap when new is unset.
        options.Attribute.OnVerificationFailed ??= legacy.OnVerificationFailed;

        // UseCancellationToken: bool has no "unset" sentinel, so we cannot tell whether the new value was
        // explicitly set. To avoid the legacy default (true) clobbering an explicit new value, only the
        // non-default legacy value (false) propagates; the new value wins in every other case.
        if (!legacy.UseCancellationToken)
        {
            options.Attribute.UseCancellationToken = false;
        }
#pragma warning restore CS0618
    }

    private static void AddTokenExtractorForOptions(this IServiceCollection services, RecaptchaOptions options)
    {
        var tokenExtractors = options.TokenExtractors;

#pragma warning disable CS0618 // Suppress obsolete warnings for legacy backward-compatibility fallbacks
        var legacy = options.AttributeOptions;

        var headerName = tokenExtractors.Header ?? legacy.ResponseTokenNameInHeader;
        if (!string.IsNullOrEmpty(headerName))
        {
            services.AddRecaptchaHeaderTokenExtractor(headerName);
        }

        var formName = tokenExtractors.Form ?? legacy.ResponseTokenNameInForm;
        if (!string.IsNullOrEmpty(formName))
        {
            services.AddRecaptchaFormTokenExtractor(formName);
        }

        var queryName = tokenExtractors.Query ?? legacy.ResponseTokenNameInQuery;
        if (!string.IsNullOrEmpty(queryName))
        {
            services.AddRecaptchaQueryTokenExtractor(queryName);
        }

        var actionArgumentName = tokenExtractors.ActionArgument;
        if (!string.IsNullOrEmpty(actionArgumentName))
        {
            services.AddRecaptchaActionArgumentsTokenExtractor(actionArgumentName);
        }

        var fromActionArguments = tokenExtractors.GetResponseTokenFromActionArguments ?? legacy.GetResponseTokenFromActionArguments;
        if (fromActionArguments is not null)
        {
            services.AddRecaptchaActionArgumentsTokenExtractor(fromActionArguments);
        }

        var fromExecutingContext = tokenExtractors.GetResponseTokenFromExecutingContext ?? legacy.GetResponseTokenFromExecutingContext;
        if (fromExecutingContext is not null)
        {
            services.AddRecaptchaExecutingContextTokenExtractor(fromExecutingContext);
        }
#pragma warning restore CS0618
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

    private static void ConfigureService(this IServiceCollection services, string? baseUrl, TimeSpan timeout, Action<HttpClient>? configureHttpClient = null)
    {
        var url = !string.IsNullOrWhiteSpace(baseUrl) ? baseUrl : DefaultBaseUrl;
        services.AddHttpClient<IRecaptchaClient, RecaptchaClient>(client =>
        {
            client.BaseAddress = new Uri(url.EndsWith('/') ? url : $"{url}/");
            if (timeout > TimeSpan.Zero)
            {
                client.Timeout = timeout;
            }
            configureHttpClient?.Invoke(client);
        });

        services.AddScoped<IRecaptchaTokenExtractionService, RecaptchaTokenExtractionService>();
        services.AddScoped<IRecaptchaVerificationService, RecaptchaVerificationService>();
        services.AddScoped<IRecaptchaVerificationResultValidationService, RecaptchaVerificationResultValidationService>();
    }
}
