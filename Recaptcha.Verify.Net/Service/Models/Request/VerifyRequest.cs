using Recaptcha.Verify.Net.Configuration;

namespace Recaptcha.Verify.Net;

/// <summary>
/// Verify reCAPTCHA response token request params.
/// </summary>
public class VerifyRequest
{
    /// <summary>
    /// The shared key between your site and reCAPTCHA.
    /// This parameter could be unspecified if secret key in <see cref="RecaptchaOptions"/> was configured.
    /// </summary>
    public string? Secret { get; set; }

    /// <summary>
    /// Required. The user response token provided by the reCAPTCHA client-side integration on your site.
    /// </summary>
    public string Response { get; set; } = null!;

    /// <summary>
    /// The user's IP address.
    /// </summary>
    public string? RemoteIp { get; set; }
}
