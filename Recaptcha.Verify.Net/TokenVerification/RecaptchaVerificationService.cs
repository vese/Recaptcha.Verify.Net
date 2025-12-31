using Microsoft.Extensions.Logging;

namespace Recaptcha.Verify.Net.TokenVerification;

internal class RecaptchaVerificationService(IRecaptchaClient recaptchaClient, ILogger<RecaptchaVerificationService> logger) : IRecaptchaVerificationService
{
    /// <inheritdoc />
    public Task<VerifyResponse> VerifyAsync(string response, string secret, string? remoteIp = null, CancellationToken cancellationToken = default) =>
        VerifyAsync(
            new VerifyRequest()
            {
                Response = response,
                Secret = secret,
                RemoteIp = remoteIp
            },
            cancellationToken);

    /// <inheritdoc />
    public async Task<VerifyResponse> VerifyAsync(VerifyRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Secret))
        {
            throw new SecretKeyNotSpecifiedException();
        }

        logger.SendingRequest(request);

        VerifyResponse result;

        try
        {
            result = await recaptchaClient.VerifyAsync(request, cancellationToken);
        }
        catch (Exception e)
        {
            throw new VerifyRequestException(e);
        }

        logger.RequestCompleted(result);

        return result;
    }
}