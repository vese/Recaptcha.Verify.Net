using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Recaptcha.Verify.Net.Attribute;

namespace Recaptcha.Verify.Net.Configuration;

/// <summary>
/// Legacy <see cref="RecaptchaAttribute"/> options. Kept for backward compatibility with the previous
/// <c>AttributeOptions</c> appsettings structure. Will be removed in a future release.
/// </summary>
[Obsolete("Will be removed in a future release. Use root RecaptchaOptions instead.")]
public class RecaptchaLegacyAttributeOptions
{
    /// <summary>
    /// <c>True</c> if need to use cancellation token for validation and verification.
    /// <para>Default value is <c>True</c>.</para>
    /// </summary>
    [Obsolete("Use RecaptchaOptions.Attribute.UseCancellationToken instead.")]
    public bool UseCancellationToken { get; set; } = true;

    /// <summary>
    /// Name of reCAPTCHA response token param in request header.
    /// </summary>
    [Obsolete("Use RecaptchaOptions.TokenExtractors.Header instead.")]
    public string? ResponseTokenNameInHeader { get; set; }

    /// <summary>
    /// Name of reCAPTCHA response token param in request query.
    /// </summary>
    [Obsolete("Use RecaptchaOptions.TokenExtractors.Query instead. Do not pass token in query parameters. It is not secure.")]
    public string? ResponseTokenNameInQuery { get; set; }

    /// <summary>
    /// Name of reCAPTCHA response token param in request form data.
    /// </summary>
    [Obsolete("Use RecaptchaOptions.TokenExtractors.Form instead.")]
    public string? ResponseTokenNameInForm { get; set; }

    /// <summary>
    /// Delegate for getting reCAPTCHA response token from action arguments.
    /// Actions argumets are mapped arguments of controller method.
    /// </summary>
    [Obsolete("Use RecaptchaOptions.TokenExtractors.GetResponseTokenFromActionArguments instead.")]
    public Func<IDictionary<string, object?>, string>? GetResponseTokenFromActionArguments { get; set; }

    /// <summary>
    /// Delegate for getting reCAPTCHA response token from executing context.
    /// </summary>
    [Obsolete("Use RecaptchaOptions.TokenExtractors.GetResponseTokenFromExecutingContext instead.")]
    public Func<ActionExecutingContext, string>? GetResponseTokenFromExecutingContext { get; set; }

    /// <summary>
    /// Delegate for handling failed verification of reCAPTCHA response token.
    /// <para>Returned <see cref="IActionResult"/> will be returned for whole request.</para>
    /// <para>Any exception could be thrown and will be propagated further.</para>
    /// </summary>
    [Obsolete("Use RecaptchaOptions.Attribute.OnVerificationFailed instead.")]
    public Func<ActionExecutingContext, string?, ValidationResult?, IActionResult>? OnVerificationFailed { get; set; }
}
