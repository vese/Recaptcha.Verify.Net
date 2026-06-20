namespace Recaptcha.Verify.Net.Configuration;

/// <summary>
/// Options for <see cref="TokenVerification.IRecaptchaVerificationService"/>.
/// </summary>
public class RecaptchaVerificationOptions
{
    /// <summary>
    /// The shared key between your site and reCAPTCHA.
    /// </summary>
    public string SecretKey { get; set; } = null!;

    /// <summary>
    /// Optional. Custom base URL for the reCAPTCHA verification API endpoint.
    /// <para>When not specified, defaults to <c>https://www.google.com/recaptcha/api</c>.</para>
    /// <para>Set this to use a custom endpoint, such as a reCAPTCHA mirror or proxy server.</para>
    /// </summary>
    public string? BaseUrl { get; set; }
}
