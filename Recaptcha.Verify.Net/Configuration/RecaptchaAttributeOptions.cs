using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Recaptcha.Verify.Net.Attribute;

namespace Recaptcha.Verify.Net.Configuration;

/// <summary>
/// Options for <see cref="RecaptchaAttribute"/>.
/// </summary>
public class RecaptchaAttributeOptions
{
    /// <summary>
    /// <c>True</c> if need to use cancellation token for validation and verification.
    /// <para>Default value is <c>True</c>.</para>
    /// </summary>
    public bool UseCancellationToken { get; set; } = true;

    /// <summary>
    /// Default returning message for unsuccessful validation and verification.
    /// </summary>
    public string VerificationFailedMessage { get; set; } = "Recaptcha verification failed";

    /// <summary>
    /// Delegate for handling failed verification of reCAPTCHA response token.
    /// <para>Returned <see cref="IActionResult"/> will be returned for whole request.</para>
    /// <para>Any exception could be thrown and will be propagated further.</para>
    /// </summary>
    public Func<ActionExecutingContext, string?, ValidationResult?, IActionResult>? OnVerificationFailed { get; set; }
}
