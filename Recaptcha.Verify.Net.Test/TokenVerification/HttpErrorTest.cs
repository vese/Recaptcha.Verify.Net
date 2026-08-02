using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.Exceptions.Processing;
using Recaptcha.Verify.Net.TokenVerification;
using Recaptcha.Verify.Net.TokenVerification.Client;
using Xunit;

namespace Recaptcha.Verify.Net.Test.TokenVerification;

/// <summary>
/// Verifies that HTTP transport failures (non-success status) preserve the status code and a
/// snippet of the response body in the surfaced exception message.
/// </summary>
public class HttpErrorTest
{
    [Fact]
    public async Task Verify_HttpFailure_PreservesStatusAndBodyInException()
    {
        // Body longer than the snippet cap (512) so capping is exercised: the head is kept,
        // the tail (beyond the cap) is not.
        var body = "err-head" + new string('x', 600) + "err-tail";
        var client = new RecaptchaClient(new HttpClient(new StubHandler(HttpStatusCode.InternalServerError, body))
        {
            BaseAddress = new Uri("https://recaptcha.example/")
        });
        var service = new RecaptchaVerificationService(
            Options.Create(new RecaptchaVerificationOptions { SecretKey = TokenVerificationFixture.ValidSecretKey }),
            client,
            NullLoggerFactory.Instance.CreateLogger<RecaptchaVerificationService>());

        var ex = await Assert.ThrowsAsync<VerifyRequestException>(() =>
            service.VerifyAsync(TokenVerificationFixture.ValidResponseTokens.First().Key));

        // Status code and a body snippet are preserved in the surfaced message.
        Assert.Contains("500", ex.Message);
        Assert.Contains("err-head", ex.Message);
        // The snippet is capped (512 chars) — the tail beyond the cap is not included.
        Assert.DoesNotContain("err-tail", ex.Message);
        // The status-bearing HttpRequestException is the inner exception.
        var httpEx = Assert.IsType<HttpRequestException>(ex.InnerException);
        Assert.Equal(HttpStatusCode.InternalServerError, httpEx.StatusCode);
    }

    /// <summary>
    /// A minimal <see cref="HttpMessageHandler"/> stub that always replies with a fixed
    /// status code and body, regardless of the request.
    /// </summary>
    private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(status)
            {
                Content = new StringContent(body)
            };
            return Task.FromResult(response);
        }
    }
}
