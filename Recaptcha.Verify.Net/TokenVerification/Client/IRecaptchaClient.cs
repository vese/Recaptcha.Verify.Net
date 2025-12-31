using Recaptcha.Verify.Net.TokenVerification.Client.Models.Request;
using Recaptcha.Verify.Net.TokenVerification.Client.Models.Response;

namespace Recaptcha.Verify.Net.TokenVerification.Client;

/// <summary>
/// Http client for verifying reCAPTCHA response token.
/// </summary>
public interface IRecaptchaClient
{
    /// <summary>
    /// Verifies reCAPTCHA response token.
    /// https://developers.google.com/recaptcha/docs/verify#api-request
    /// </summary>
    /// <param name="request">Verify reCAPTCHA response token request params.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="VerifyResponse"/> verification response.</returns>
    Task<VerifyResponse> VerifyAsync(VerifyRequest request, CancellationToken cancellationToken = default);
}