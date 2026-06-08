using Microsoft.Extensions.DependencyInjection;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.TokenVerification.Client;
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
        AssertBaseAddress(DefaultBaseUrl, o => o.SecretKey = SecretKey);

    [Fact]
    public void AddRecaptcha_CustomBaseUrl_WhenSpecified() =>
        AssertBaseAddress(CustomBaseUrlWithSlash, o =>
        {
            o.SecretKey = SecretKey;
            o.BaseUrl = CustomBaseUrl;
        });

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddRecaptcha_DefaultBaseUrl_WhenNullOrEmpty(string? baseUrl) =>
        AssertBaseAddress(DefaultBaseUrl, o =>
        {
            o.SecretKey = SecretKey;
            o.BaseUrl = baseUrl;
        });

    [Fact]
    public void AddRecaptcha_AppendsTrailingSlash_WhenMissing() =>
        AssertBaseAddress(CustomBaseUrlWithSlash, o =>
        {
            o.SecretKey = SecretKey;
            o.BaseUrl = CustomBaseUrl;
        });

    [Fact]
    public void AddRecaptcha_PreservesTrailingSlash_WhenPresent() =>
        AssertBaseAddress(CustomBaseUrlWithSlash, o =>
        {
            o.SecretKey = SecretKey;
            o.BaseUrl = CustomBaseUrlWithSlash;
        });

    private static void AssertBaseAddress(string expectedUrl, Action<RecaptchaOptions> configure)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRecaptcha(configure);

        using var provider = services.BuildServiceProvider();
        var client = (RecaptchaClient)provider.GetRequiredService<IRecaptchaClient>();

        Assert.Equal(new Uri(expectedUrl), GetBaseAddress(client));
    }

    private static Uri GetBaseAddress(RecaptchaClient client)
    {
        var httpClientField = client.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .First(f => f.FieldType == typeof(HttpClient));

        var httpClient = (HttpClient)httpClientField.GetValue(client)!;
        return httpClient.BaseAddress!;
    }
}
