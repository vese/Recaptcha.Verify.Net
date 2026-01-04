namespace Recaptcha.Verify.Net.TokenVerification;

/// <summary>
/// Service for verifying reCAPTCHA response token.
/// </summary>
public interface IRecaptchaVerificationService
{
    /// <summary>
    /// Verifies reCAPTCHA response token.
    /// https://developers.google.com/recaptcha/docs/verify#api-request
    /// </summary>
    /// <param name="response">The user response token provided by the reCAPTCHA client-side integration on your site.</param>
    /// <param name="remoteIp">The user's IP address.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.
    /// The task result contains verification response <see cref="VerifyResponse"/>.</returns>
    /// <exception cref="SecretKeyNotSpecifiedException">
    /// This exception is thrown when secret key was not specified in options or request params.
    /// </exception>
    /// <exception cref="EmptyCaptchaAnswerException">
    /// This exception is thrown when user response token was not specified.
    /// </exception>
    /// <exception cref="VerifyRequestException">
    /// This exception is thrown when verify request failed.
    /// </exception>
    Task<VerifyResponse> VerifyAsync(string response, string? remoteIp = null, CancellationToken cancellationToken = default);
}
