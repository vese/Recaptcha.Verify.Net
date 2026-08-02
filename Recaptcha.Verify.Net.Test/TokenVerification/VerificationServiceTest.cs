using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.Exceptions.Configuration;
using Recaptcha.Verify.Net.Exceptions.Processing;
using Recaptcha.Verify.Net.TokenVerification;
using Recaptcha.Verify.Net.TokenVerification.Client;
using System.Net;
using Xunit;

namespace Recaptcha.Verify.Net.Test.TokenVerification;

public class VerificationServiceTest
{
    public static TheoryData<string?> EmptyStrings => new(null, string.Empty, "   ");

    public static TheoryData<string, float> GetResponseTokens()
    {
        var theoryData = new TheoryData<string, float>();

        foreach (var tokenScore in TokenVerificationFixture.ValidResponseTokens)
        {
            theoryData.Add(tokenScore.Key, tokenScore.Value);
        }

        return theoryData;
    }

    [Theory]
    [MemberData(nameof(EmptyStrings))]
    public async Task Verify_MissingSecretKey_Throws(string? secretKey)
    {
        var verificationService = VerificationServiceFixture.Create(secretKey!);

        await Assert.ThrowsAsync<SecretKeyNotSpecifiedException>(() => verificationService.VerifyAsync(TokenVerificationFixture.ValidResponseTokens.First().Key));
    }

    [Theory]
    [MemberData(nameof(EmptyStrings))]
    public async Task Verify_EmptyResponse_Throws(string? response)
    {
        var verificationService = VerificationServiceFixture.Create(TokenVerificationFixture.ValidSecretKey);

        await Assert.ThrowsAsync<EmptyCaptchaAnswerException>(() => verificationService.VerifyAsync(response!));
    }

    [Fact]
    public async Task Verify_ClientException_Throws()
    {
        var verificationService = VerificationServiceFixture.Create(TokenVerificationFixture.ValidSecretKey, true);

        await Assert.ThrowsAsync<VerifyRequestException>(() => verificationService.VerifyAsync(TokenVerificationFixture.ValidResponseTokens.First().Key));
    }

    [Fact]
    public async Task Verify_InvalidResponseToken_ReturnsVerificationResult()
    {
        var verificationService = VerificationServiceFixture.Create(TokenVerificationFixture.ValidSecretKey);

        var verificationResult = await verificationService.VerifyAsync(TokenVerificationFixture.InvalidResponseToken);

        Assert.NotNull(verificationResult);
        Assert.False(verificationResult.Success);
    }

    [Theory]
    [MemberData(nameof(GetResponseTokens))]
    public async Task Verify_ValidResponseToken_ReturnsVerificationResult(string responseToken, float score)
    {
        var verificationService = VerificationServiceFixture.Create(TokenVerificationFixture.ValidSecretKey);

        var verificationResult = await verificationService.VerifyAsync(responseToken);

        Assert.NotNull(verificationResult);
        Assert.True(verificationResult.Success);
        Assert.Equal(score, verificationResult.Score);
    }

    [Fact]
    public async Task Verify_HttpTimeout_ThrowsVerifyRequestException()
    {
        using var handler = new DelayedHandler(TimeSpan.FromSeconds(30));
        using var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://www.google.com/recaptcha/api/"),
            Timeout = TimeSpan.FromMilliseconds(100),
        };
        var client = new RecaptchaClient(http);
        var service = new RecaptchaVerificationService(
            Options.Create(new RecaptchaVerificationOptions { SecretKey = TokenVerificationFixture.ValidSecretKey }),
            client,
            NullLoggerFactory.Instance.CreateLogger<RecaptchaVerificationService>());

        var ex = await Assert.ThrowsAsync<VerifyRequestException>(() => service.VerifyAsync("token", "1.2.3.4"));

        // HttpClient.Timeout surfaces as OperationCanceledException; the service wraps it as VerifyRequestException.
        Assert.IsAssignableFrom<OperationCanceledException>(ex.InnerException);
    }

    /// <summary>
    /// A stub handler that delays the configured duration before responding, so the caller's
    /// HttpClient.Timeout elapses first.
    /// </summary>
    private sealed class DelayedHandler(TimeSpan delay) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(delay, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
