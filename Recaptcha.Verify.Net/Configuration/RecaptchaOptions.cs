using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Recaptcha.Verify.Net.Attribute;
using Recaptcha.Verify.Net.VerificationResultValidation.Models;

namespace Recaptcha.Verify.Net.Configuration;

/// <summary>
/// Recaptcha options. Root configuration model used for appsettings mapping.
/// <para>Settings are grouped into focused sub-options: <see cref="Verification"/>, <see cref="Validation"/>,
/// <see cref="Attribute"/> and <see cref="TokenExtractors"/>.</para>
/// </summary>
public class RecaptchaOptions
{
    /// <summary>
    /// Options for token verification with Google's reCAPTCHA API.
    /// </summary>
    public RecaptchaVerificationOptions Verification { get; set; } = new();

    /// <summary>
    /// Options for validating the reCAPTCHA verification result.
    /// </summary>
    public RecaptchaValidationOptions Validation { get; set; } = new();

    /// <summary>
    /// Options for <see cref="RecaptchaAttribute"/>.
    /// </summary>
    public RecaptchaAttributeOptions Attribute { get; set; } = new();

    /// <summary>
    /// Options for configuring token extractors that retrieve reCAPTCHA response tokens from requests.
    /// </summary>
    public RecaptchaTokenExtractorOptions TokenExtractors { get; set; } = new();

    /// <summary>
    /// Optional. Custom base URL for the reCAPTCHA verification API endpoint.
    /// </summary>
    [Obsolete($"Use {nameof(Verification)}.{nameof(RecaptchaVerificationOptions.BaseUrl)} instead.")]
    public string? BaseUrl
    {
        get => Verification.BaseUrl;
        set => Verification.BaseUrl = value;
    }

    /// <summary>
    /// Legacy <see cref="RecaptchaAttribute"/> options. Kept for backward compatibility with the previous
    /// <c>AttributeOptions</c> appsettings structure. Will be removed in a future release.
    /// </summary>
    [Obsolete("Will be removed in a future release. Use the corresponding nested options instead.")]
    public RecaptchaLegacyAttributeOptions AttributeOptions { get; set; } = new();

    /// <summary>
    /// The shared key between your site and reCAPTCHA.
    /// </summary>
    [Obsolete($"Use {nameof(Verification)}.{nameof(RecaptchaVerificationOptions.SecretKey)} instead.")]
    public string SecretKey
    {
        get => Verification.SecretKey;
        set => Verification.SecretKey = value;
    }

    /// <summary>
    /// Optional. Action to check for V3 Recaptcha request.
    /// <para>Action specified in <see cref="IRecaptchaVerificationResultValidationService.Validate"/>
    /// or in <see cref="RecaptchaAttribute"/> will be used instead of this value.</para>
    /// </summary>
    [Obsolete($"Use {nameof(Validation)}.{nameof(RecaptchaValidationOptions.Action)} instead.")]
    public string? Action
    {
        get => Validation.Action;
        set => Validation.Action = value;
    }

    /// <summary>
    /// Optional. Score threshold for V3 Recaptcha (0.0 - 1.0).
    /// <para>Score threshold specified in <see cref="IRecaptchaVerificationResultValidationService.Validate"/>
    /// or in <see cref="RecaptchaAttribute"/> will be used instead of this value.</para>
    /// </summary>
    [Obsolete($"Use {nameof(Validation)}.{nameof(RecaptchaValidationOptions.ScoreThreshold)} instead.")]
    public float? ScoreThreshold
    {
        get => Validation.ScoreThreshold;
        set => Validation.ScoreThreshold = value;
    }

    /// <summary>
    /// Optional. Map of actions score thresholds for V3 Recaptcha.
    /// <para>Score threshold specified in <see cref="IRecaptchaVerificationResultValidationService.Validate"/>
    /// or in <see cref="RecaptchaAttribute"/> will be used instead of this value.</para>
    /// </summary>
    [Obsolete($"Use {nameof(Validation)}.{nameof(RecaptchaValidationOptions.ActionsScoreThresholds)} instead.")]
    public IReadOnlyDictionary<string, float>? ActionsScoreThresholds
    {
        get => Validation.ActionsScoreThresholds;
        set => Validation.ActionsScoreThresholds = value;
    }

    /// <summary>
    /// Default returning message for unsuccessful validation and verification.
    /// <para>
    /// This message would be replaced by value processed by <see cref="RecaptchaAttributeOptions.OnVerificationFailed"/>.
    /// </para>
    /// </summary>
    [Obsolete($"Use {nameof(Attribute)}.{nameof(RecaptchaAttributeOptions.VerificationFailedMessage)} instead.")]
    public string VerificationFailedMessage
    {
        get => Attribute.VerificationFailedMessage;
        set => Attribute.VerificationFailedMessage = value;
    }
}
