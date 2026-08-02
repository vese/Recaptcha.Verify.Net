using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.TokenExtraction;
using Recaptcha.Verify.Net.TokenVerification.Client;
using Recaptcha.Verify.Net.VerificationResultValidation.Models;
using Xunit;

namespace Recaptcha.Verify.Net.Test.Configuration;

public class ConfigurationExtensionsTest
{
    private const string SecretKey = "test-secret";
    private const string DefaultBaseUrl = "https://www.google.com/recaptcha/api/";
    private const string CustomBaseUrl = "https://recaptcha-proxy.example.com/recaptcha/api";
    private const string CustomBaseUrlWithSlash = "https://recaptcha-proxy.example.com/recaptcha/api/";

    [Fact]
    public void AddRecaptcha_DefaultBaseUrl_WhenNotSpecified() =>
        AssertBaseAddress(DefaultBaseUrl, o => o.Verification.SecretKey = SecretKey);

    [Fact]
    public void AddRecaptcha_CustomBaseUrl_WhenSpecified() =>
        AssertBaseAddress(CustomBaseUrlWithSlash, o =>
        {
            o.Verification.SecretKey = SecretKey;
            o.Verification.BaseUrl = CustomBaseUrl;
        });

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddRecaptcha_DefaultBaseUrl_WhenNullOrEmpty(string? baseUrl) =>
        AssertBaseAddress(DefaultBaseUrl, o =>
        {
            o.Verification.SecretKey = SecretKey;
            o.Verification.BaseUrl = baseUrl;
        });

    [Fact]
    public void AddRecaptcha_AppendsTrailingSlash_WhenMissing() =>
        AssertBaseAddress(CustomBaseUrlWithSlash, o =>
        {
            o.Verification.SecretKey = SecretKey;
            o.Verification.BaseUrl = CustomBaseUrl;
        });

    [Fact]
    public void AddRecaptcha_PreservesTrailingSlash_WhenPresent() =>
        AssertBaseAddress(CustomBaseUrlWithSlash, o =>
        {
            o.Verification.SecretKey = SecretKey;
            o.Verification.BaseUrl = CustomBaseUrlWithSlash;
        });

    [Fact]
    public void AddRecaptcha_RegistersNestedOptionsAsFocused()
    {
        var services = new ServiceCollection();
        services.AddRecaptcha(o =>
        {
            o.Verification.SecretKey = SecretKey;
            o.Verification.BaseUrl = CustomBaseUrl;
            o.Validation.Action = "login";
            o.Validation.ScoreThreshold = 0.5f;
            o.Validation.ActionsScoreThresholds = new Dictionary<string, float> { ["login"] = 0.8f };
            o.Attribute.UseCancellationToken = false;
            o.Attribute.VerificationFailedMessage = "Custom message";
            o.Attribute.OnVerificationFailed = (_, _, _) => new EmptyResult();
        });

        using var provider = services.BuildServiceProvider();

        var verificationOptions = provider.GetRequiredService<IOptions<RecaptchaVerificationOptions>>().Value;
        Assert.Equal(SecretKey, verificationOptions.SecretKey);
        Assert.Equal(CustomBaseUrl, verificationOptions.BaseUrl);

        var validationOptions = provider.GetRequiredService<IOptions<RecaptchaValidationOptions>>().Value;
        Assert.Equal("login", validationOptions.Action);
        Assert.Equal(0.5f, validationOptions.ScoreThreshold);
        Assert.Equal(0.8f, validationOptions.ActionsScoreThresholds!["login"]);

        var attributeOptions = provider.GetRequiredService<IOptions<RecaptchaAttributeOptions>>().Value;
        Assert.False(attributeOptions.UseCancellationToken);
        Assert.Equal("Custom message", attributeOptions.VerificationFailedMessage);
        Assert.NotNull(attributeOptions.OnVerificationFailed);
    }

    [Fact]
    public void AddRecaptcha_LegacyFlatOptions_FlowThroughObsoleteProxy()
    {
        var services = new ServiceCollection();
        services.AddLogging();
#pragma warning disable CS0618 // Verify backward compatibility of the obsolete flat root fields
        services.AddRecaptcha(o =>
        {
            o.SecretKey = SecretKey;
            o.Action = "login";
            o.ScoreThreshold = 0.5f;
            o.VerificationFailedMessage = "Custom message";
        });
#pragma warning restore CS0618

        using var provider = services.BuildServiceProvider();

        var verificationOptions = provider.GetRequiredService<IOptions<RecaptchaVerificationOptions>>().Value;
        Assert.Equal(SecretKey, verificationOptions.SecretKey);

        var validationOptions = provider.GetRequiredService<IOptions<RecaptchaValidationOptions>>().Value;
        Assert.Equal("login", validationOptions.Action);
        Assert.Equal(0.5f, validationOptions.ScoreThreshold);

        var attributeOptions = provider.GetRequiredService<IOptions<RecaptchaAttributeOptions>>().Value;
        Assert.Equal("Custom message", attributeOptions.VerificationFailedMessage);
    }

    [Fact]
    public void AddRecaptcha_LegacyAttributeOptions_PropagateOntoAttribute()
    {
        Func<ActionExecutingContext, string?, ValidationResult?, IActionResult> legacyHandler = (_, _, _) => new EmptyResult();

        var services = new ServiceCollection();
        services.AddLogging();
#pragma warning disable CS0618 // Verify backward compatibility of the obsolete legacy attribute options
        services.AddRecaptcha(o =>
        {
            o.Verification.SecretKey = SecretKey;
            o.AttributeOptions.UseCancellationToken = false;
            o.AttributeOptions.OnVerificationFailed = legacyHandler;
        });
#pragma warning restore CS0618

        using var provider = services.BuildServiceProvider();
        var attributeOptions = provider.GetRequiredService<IOptions<RecaptchaAttributeOptions>>().Value;

        Assert.False(attributeOptions.UseCancellationToken);
        Assert.Same(legacyHandler, attributeOptions.OnVerificationFailed);
    }

    [Fact]
    public void AddRecaptcha_LegacyOnVerificationFailed_LosesToNewAttribute()
    {
        Func<ActionExecutingContext, string?, ValidationResult?, IActionResult> legacyHandler = (_, _, _) => new BadRequestResult();
        Func<ActionExecutingContext, string?, ValidationResult?, IActionResult> newHandler = (_, _, _) => new EmptyResult();

        var services = new ServiceCollection();
        services.AddLogging();
#pragma warning disable CS0618 // Verify backward compatibility of the obsolete legacy attribute options
        services.AddRecaptcha(o =>
        {
            o.Verification.SecretKey = SecretKey;
            o.Attribute.OnVerificationFailed = newHandler;
            o.AttributeOptions.OnVerificationFailed = legacyHandler;
        });
#pragma warning restore CS0618

        using var provider = services.BuildServiceProvider();
        var attributeOptions = provider.GetRequiredService<IOptions<RecaptchaAttributeOptions>>().Value;

        // The new (canonical) delegate wins; the legacy delegate is the fallback only.
        Assert.Same(newHandler, attributeOptions.OnVerificationFailed);
    }

    [Fact]
    public void AddRecaptcha_TokenExtractors_Header_RegistersHeaderExtractor()
    {
        var services = new ServiceCollection();
        services.AddRecaptcha(o =>
        {
            o.Verification.SecretKey = SecretKey;
            o.TokenExtractors.Header = "X-Recaptcha-Token";
        });

        using var provider = services.BuildServiceProvider();
        var extractors = provider.GetServices<IRecaptchaTokenExtractor>().ToList();

        Assert.Single(extractors);
        Assert.IsType<HeaderTokenExtractor>(extractors[0]);
    }

    [Fact]
    public void AddRecaptcha_TokenExtractors_Form_RegistersFormExtractor()
    {
        var services = new ServiceCollection();
        services.AddRecaptcha(o =>
        {
            o.Verification.SecretKey = SecretKey;
            o.TokenExtractors.Form = "recaptchaToken";
        });

        using var provider = services.BuildServiceProvider();
        var extractors = provider.GetServices<IRecaptchaTokenExtractor>().ToList();

        Assert.Single(extractors);
        Assert.IsType<FormTokenExtractor>(extractors[0]);
    }

    [Fact]
    public void AddRecaptcha_TokenExtractors_Query_RegistersQueryExtractor()
    {
#pragma warning disable CS0618 // Query is obsolete (not secure), verifying backward-compatible registration
        var services = new ServiceCollection();
        services.AddRecaptcha(o =>
        {
            o.Verification.SecretKey = SecretKey;
            o.TokenExtractors.Query = "recaptchaToken";
        });

        using var provider = services.BuildServiceProvider();
        var extractors = provider.GetServices<IRecaptchaTokenExtractor>().ToList();

        Assert.Single(extractors);
        Assert.IsType<QueryTokenExtractor>(extractors[0]);
#pragma warning restore CS0618
    }

    [Fact]
    public void AddRecaptcha_TokenExtractors_ActionArgumentsDelegate_RegistersExtractor()
    {
        var services = new ServiceCollection();
        services.AddRecaptcha(o =>
        {
            o.Verification.SecretKey = SecretKey;
            o.TokenExtractors.GetResponseTokenFromActionArguments = _ => "token";
        });

        using var provider = services.BuildServiceProvider();
        var extractors = provider.GetServices<IRecaptchaTokenExtractor>().ToList();

        Assert.Single(extractors);
        Assert.IsType<ActionArgumentsTokenExtractor>(extractors[0]);
    }

    [Fact]
    public void AddRecaptcha_TokenExtractors_ActionArgumentName_RegistersExtractor()
    {
        var services = new ServiceCollection();
        services.AddRecaptcha(o =>
        {
            o.Verification.SecretKey = SecretKey;
            o.TokenExtractors.ActionArgument = "recaptchaToken";
        });

        using var provider = services.BuildServiceProvider();
        var extractors = provider.GetServices<IRecaptchaTokenExtractor>().ToList();

        Assert.Single(extractors);
        Assert.IsType<ActionArgumentsTokenExtractor>(extractors[0]);
    }

    [Fact]
    public void AddRecaptcha_TokenExtractors_ExecutingContextDelegate_RegistersExtractor()
    {
        var services = new ServiceCollection();
        services.AddRecaptcha(o =>
        {
            o.Verification.SecretKey = SecretKey;
            o.TokenExtractors.GetResponseTokenFromExecutingContext = _ => "token";
        });

        using var provider = services.BuildServiceProvider();
        var extractors = provider.GetServices<IRecaptchaTokenExtractor>().ToList();

        Assert.Single(extractors);
        Assert.IsType<ExecutingContextTokenExtractor>(extractors[0]);
    }

    [Fact]
    public void AddRecaptcha_TokenExtractors_TakesPrecedenceOverAttributeOptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();
#pragma warning disable CS0618
        services.AddRecaptcha(o =>
        {
            o.Verification.SecretKey = SecretKey;
            o.TokenExtractors.Header = "X-New-Header";
            o.AttributeOptions.ResponseTokenNameInHeader = "X-Old-Header";
        });
#pragma warning restore CS0618

        using var provider = services.BuildServiceProvider();
        var extractors = provider.GetServices<IRecaptchaTokenExtractor>().ToList();

        Assert.Single(extractors);
    }

    [Fact]
    public void AddRecaptcha_FallsBackToAttributeOptions_WhenTokenExtractorsEmpty()
    {
        var services = new ServiceCollection();
        services.AddLogging();
#pragma warning disable CS0618
        services.AddRecaptcha(o =>
        {
            o.Verification.SecretKey = SecretKey;
            o.AttributeOptions.ResponseTokenNameInHeader = "X-Old-Header";
        });
#pragma warning restore CS0618

        using var provider = services.BuildServiceProvider();
        var extractors = provider.GetServices<IRecaptchaTokenExtractor>().ToList();

        Assert.Single(extractors);
        Assert.IsType<HeaderTokenExtractor>(extractors[0]);
    }

    [Fact]
    public void AddRecaptcha_TokenExtractors_FromConfigurationSection()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Recaptcha:Verification:SecretKey", SecretKey },
                { "Recaptcha:TokenExtractors:Header", "X-Recaptcha-Token" },
                { "Recaptcha:TokenExtractors:Form", "recaptchaToken" },
                { "Recaptcha:TokenExtractors:Query", "recaptchaToken" },
                { "Recaptcha:TokenExtractors:ActionArgument", "recaptchaToken" },
            })
            .Build();

        var services = new ServiceCollection();
        services.AddRecaptcha(config.GetSection("Recaptcha"));

        using var provider = services.BuildServiceProvider();
        var extractors = provider.GetServices<IRecaptchaTokenExtractor>().ToList();

        Assert.Equal(4, extractors.Count);
        Assert.Contains(extractors, e => e is HeaderTokenExtractor);
        Assert.Contains(extractors, e => e is FormTokenExtractor);
#pragma warning disable CS0618 // QueryTokenExtractor is obsolete (not secure)
        Assert.Contains(extractors, e => e is QueryTokenExtractor);
#pragma warning restore CS0618
        Assert.Contains(extractors, e => e is ActionArgumentsTokenExtractor);

        var verificationOptions = provider.GetRequiredService<IOptions<RecaptchaVerificationOptions>>().Value;
        Assert.Equal(SecretKey, verificationOptions.SecretKey);
    }

    [Fact]
    public void AddRecaptcha_ActionsScoreThresholds_BindsFromNestedConfigurationSection()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Recaptcha:Verification:SecretKey", SecretKey },
                { "Recaptcha:Validation:ActionsScoreThresholds:login", "0.8" },
                { "Recaptcha:Validation:ActionsScoreThresholds:comment", "0.3" },
            })
            .Build();

        var services = new ServiceCollection();
        services.AddRecaptcha(config.GetSection("Recaptcha"));

        using var provider = services.BuildServiceProvider();
        var validationOptions = provider.GetRequiredService<IOptions<RecaptchaValidationOptions>>().Value;

        Assert.NotNull(validationOptions.ActionsScoreThresholds);
        Assert.Equal(0.8f, validationOptions.ActionsScoreThresholds!["login"]);
        Assert.Equal(0.3f, validationOptions.ActionsScoreThresholds!["comment"]);
    }

    [Fact]
    public void AddRecaptcha_ActionsScoreThresholds_BindsFromLegacyFlatConfigurationSection()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Recaptcha:Verification:SecretKey", SecretKey },
                { "Recaptcha:ActionsScoreThresholds:login", "0.8" },
            })
            .Build();

        var services = new ServiceCollection();
        services.AddRecaptcha(config.GetSection("Recaptcha"));

        using var provider = services.BuildServiceProvider();
        var validationOptions = provider.GetRequiredService<IOptions<RecaptchaValidationOptions>>().Value;

        Assert.NotNull(validationOptions.ActionsScoreThresholds);
        Assert.Equal(0.8f, validationOptions.ActionsScoreThresholds!["login"]);
    }

    [Fact]
    public void AddRecaptcha_NoExtractors_WhenNothingConfigured()
    {
        var services = new ServiceCollection();
        services.AddRecaptcha(o => o.Verification.SecretKey = SecretKey);

        using var provider = services.BuildServiceProvider();
        var extractors = provider.GetServices<IRecaptchaTokenExtractor>().ToList();

        Assert.Empty(extractors);
    }

    [Fact]
    public void AddRecaptcha_AppliesTimeoutToHttpClient_WhenConfigured()
    {
        var expected = TimeSpan.FromSeconds(7);
        var services = new ServiceCollection();
        services.AddRecaptcha(o =>
        {
            o.Verification.SecretKey = SecretKey;
            o.Verification.Timeout = expected;
        });

        using var provider = services.BuildServiceProvider();
        var client = (RecaptchaClient)provider.GetRequiredService<IRecaptchaClient>();

        Assert.Equal(expected, GetHttpClient(client).Timeout);
    }

    [Fact]
    public void AddRecaptcha_DefaultTimeout_IsTenSeconds()
    {
        var services = new ServiceCollection();
        services.AddRecaptcha(o => o.Verification.SecretKey = SecretKey);

        using var provider = services.BuildServiceProvider();
        var client = (RecaptchaClient)provider.GetRequiredService<IRecaptchaClient>();

        Assert.Equal(TimeSpan.FromSeconds(10), GetHttpClient(client).Timeout);
    }

    [Fact]
    public void AddRecaptcha_KeepsHttpClientDefaultTimeout_WhenTimeoutIsZero()
    {
        var services = new ServiceCollection();
        services.AddRecaptcha(o =>
        {
            o.Verification.SecretKey = SecretKey;
            o.Verification.Timeout = TimeSpan.Zero; // opt out: keep HttpClient default
        });

        using var provider = services.BuildServiceProvider();
        var client = (RecaptchaClient)provider.GetRequiredService<IRecaptchaClient>();

        Assert.Equal(TimeSpan.FromSeconds(100), GetHttpClient(client).Timeout);
    }

    [Fact]
    public void AddRecaptcha_AppliesConfigureHttpClientAction()
    {
        var services = new ServiceCollection();
        services.AddRecaptcha(
            o => o.Verification.SecretKey = SecretKey,
            configureHttpClient: c =>
            {
                c.DefaultRequestHeaders.Add("X-Test", "rv-0004");
                c.Timeout = TimeSpan.FromSeconds(3);
            });

        using var provider = services.BuildServiceProvider();
        var client = (RecaptchaClient)provider.GetRequiredService<IRecaptchaClient>();
        var http = GetHttpClient(client);

        // The action runs after the library defaults, so it can add headers and override Timeout.
        Assert.Equal("rv-0004", http.DefaultRequestHeaders.GetValues("X-Test").First());
        Assert.Equal(TimeSpan.FromSeconds(3), http.Timeout);
    }

    private static void AssertBaseAddress(string expectedUrl, Action<RecaptchaOptions> configure)
    {
        var services = new ServiceCollection();
        services.AddRecaptcha(configure);

        using var provider = services.BuildServiceProvider();
        var client = (RecaptchaClient)provider.GetRequiredService<IRecaptchaClient>();

        Assert.Equal(new Uri(expectedUrl), GetBaseAddress(client));
    }

    private static HttpClient GetHttpClient(RecaptchaClient client)
    {
        var httpClientField = client.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .First(f => f.FieldType == typeof(HttpClient));

        return (HttpClient)httpClientField.GetValue(client)!;
    }

    private static Uri GetBaseAddress(RecaptchaClient client) => GetHttpClient(client).BaseAddress!;
}
