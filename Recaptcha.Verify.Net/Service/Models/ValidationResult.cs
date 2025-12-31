namespace Recaptcha.Verify.Net.Service.Models;

/// <summary>
/// Result of validating of verification response.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// <c>True</c> if successfully verified.
    /// </summary>
    public required bool ResponseSuccessful { get; init; }

    /// <summary>
    /// <c>True</c> if reCAPTCHA v3 is used.
    /// </summary>
    public required bool IsV3 { get; init; }

    /// <summary>
    /// <c>True</c> if action matches specified value.
    /// For reCAPTCHA v2 value is <c>False</c>.
    /// </summary>
    public bool ActionMatches { get; set; }

    /// <summary>
    /// <c>True</c> if score satisfies specified threshold.
    /// For reCAPTCHA v2 value is <c>False</c>.
    /// </summary>
    public bool ScoreSatisfies { get; set; }

    /// <summary>
    /// <c>True</c> if successfully verified and satisfies specified requirements.
    /// </summary>
    public bool Success => ResponseSuccessful && (!IsV3 || ActionMatches && ScoreSatisfies);
}
