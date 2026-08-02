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

    /// <summary>
    /// Optional. Maximum time to wait for the outbound siteverify HTTP call to complete.
    /// <para>Bounds the request so a hung Google endpoint cannot block the caller indefinitely.</para>
    /// <para>Defaults to 10 seconds. Set to <see cref="TimeSpan.Zero"/> to keep the <see cref="HttpClient"/> default timeout.</para>
    /// <para>When the timeout elapses the request fails and surfaces as <see cref="Recaptcha.Verify.Net.Exceptions.Processing.VerifyRequestException"/>.</para>
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}
