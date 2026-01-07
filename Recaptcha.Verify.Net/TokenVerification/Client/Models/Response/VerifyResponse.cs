using Recaptcha.Verify.Net.TokenVerification.Client.Helpers;
using System.Text.Json.Serialization;

namespace Recaptcha.Verify.Net.TokenVerification.Client.Models.Response;

/// <summary>
/// Response of reCAPTCHA response token verification.
/// </summary>
public class VerifyResponse
{
    /// <summary>
    /// Indicates whether this request was a valid reCAPTCHA token for your site.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Used for reCAPTCHA v3.
    /// The score for this request (0.0 - 1.0).
    /// </summary>
    [JsonPropertyName("score")]
    public float? Score { get; set; }

    /// <summary>
    /// Used for reCAPTCHA v3.
    /// The action name for this request (important to verify).
    /// </summary>
    [JsonPropertyName("action")]
    public string? Action { get; set; }

    /// <summary>
    /// Timestamp of the challenge load (ISO format yyyy-MM-dd'T'HH:mm:ssZZ).
    /// </summary>
    [JsonPropertyName("challenge_ts")]
    public DateTime ChallengeTs { get; set; }

    /// <summary>
    /// The hostname of the site where the reCAPTCHA was solved.
    /// </summary>
    [JsonPropertyName("hostname")]
    public string? Hostname { get; set; }

    /// <summary>
    /// The package name of the app where the reCAPTCHA was solved.
    /// </summary>
    [JsonPropertyName("apk_package_name")]
    public string? ApkPackageName { get; set; }

    /// <summary>
    /// List of error codes.
    /// <list type="table">
    /// <listheader>
    /// <term>Error code</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term>missing-input-secret</term>
    /// <description>The secret parameter is missing.</description>
    /// </item>
    /// <item>
    /// <term>invalid-input-secret</term>
    /// <description>The secret parameter is invalid or malformed.</description>
    /// </item>
    /// <item>
    /// <term>missing-input-response</term>
    /// <description>The response parameter is missing.</description>
    /// </item>
    /// <item>
    /// <term>invalid-input-response</term>
    /// <description>The response parameter is invalid or malformed.</description>
    /// </item>
    /// <item>
    /// <term>bad-request</term>
    /// <description>The request is invalid or malformed.</description>
    /// </item>
    /// <item>
    /// <term>timeout-or-duplicate</term>
    /// <description>The response is no longer valid: either is too old or has been used previously.</description>
    /// </item>
    /// </list>
    /// </summary>
    [JsonPropertyName("error-codes")]
    public IReadOnlyCollection<string>? ErrorCodes { get; set; }

    /// <summary>
    /// Returns list of the verify errors.
    /// </summary>
    /// <exception cref="UnknownErrorKeyException">
    /// This exception is thrown when verification response error key is unknown.
    /// </exception>
    [JsonIgnore]
    public IReadOnlyCollection<VerifyError>? Errors => VerifyErrorHelper.GetVerifyErrors(ErrorCodes);

    /// <summary>
    /// <c>True</c> if reCAPTCHA v3 is used.
    /// </summary>
    [JsonIgnore]
    public bool IsV3 => Score.HasValue;
}
