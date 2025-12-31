namespace Recaptcha.Verify.Net.Client.Models.Request;

/// <summary>
/// Verify reCAPTCHA response token request params.
/// </summary>
public class VerifyRequest
{
    /// <summary>
    /// The shared key between your site and reCAPTCHA.
    /// </summary>
    public required string Secret { get; set; }

    /// <summary>
    /// The user response token provided by the reCAPTCHA client-side integration on your site.
    /// </summary>
    public required string Response { get; set; }

    /// <summary>
    /// The user's IP address.
    /// </summary>
    public string? RemoteIp { get; set; }
}
