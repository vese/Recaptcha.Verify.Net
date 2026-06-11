using Recaptcha.Verify.Net.Attribute;

namespace Recaptcha.Verify.Net.Configuration;

/// <summary>
/// Recaptcha options.
/// </summary>
public class RecaptchaOptions
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
    /// Optional. Action to check for V3 Recaptcha request.
    /// <para>Action specified in <see cref="IRecaptchaVerificationResultValidationService.Validate"/>
    /// or in <see cref="RecaptchaAttribute"/> will be used instead of this value.</para>
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Optional. Score threshold for V3 Recaptcha (0.0 - 1.0).
    /// <para>Score threshold specified in <see cref="IRecaptchaVerificationResultValidationService.Validate"/>
    /// or in <see cref="RecaptchaAttribute"/> will be used instead of this value.</para>
    /// </summary>
    public float? ScoreThreshold { get; set; }

    /// <summary>
    /// Optional. Map of actions score thresholds for V3 Recaptcha.
    /// <para>Score threshold specified in <see cref="IRecaptchaVerificationResultValidationService.Validate"/>
    /// or in <see cref="RecaptchaAttribute"/> will be used instead of this value.</para>
    /// </summary>
    public IReadOnlyDictionary<string, float>? ActionsScoreThresholds { get; set; }

    /// <summary>
    /// Default returning message for unsuccessful validation and checking.
    /// <para>
    /// This message would be replaced by value processed by <see cref="RecaptchaAttributeOptions.OnVerificationFailed"/>.
    /// </para>
    /// </summary>
    public string VerificationFailedMessage { get; set; } = "Recaptcha verification failed";

    /// <summary>
    /// Options for <see cref="RecaptchaAttribute"/>.
    /// </summary>
    public RecaptchaAttributeOptions AttributeOptions { get; set; } = new RecaptchaAttributeOptions();
}
