namespace Recaptcha.Verify.Net.VerificationResultValidation;

/// <summary>
/// Service for validation of reCAPTCHA response token verification result.
/// </summary>
public interface IRecaptchaVerificationResultValidationService
{
    /// <summary>
    /// Validates reCAPTCHA response token verification result.
    /// <para>For v3 if not specified takes score threshold and action from options <see cref="RecaptchaOptions"/> .</para>
    /// </summary>
    /// <param name="response">Result of reCAPTCHA response token verification.</param>
    /// <param name="action">Action that the action from the response should be equal to.</param>
    /// <param name="score">Score threshold.</param>
    /// <returns>Result of validation of reCAPCTHA token verification.</returns>
    /// <exception cref="EmptyActionException">
    /// This exception is thrown when the action passed in function is empty.
    /// </exception>
    /// <exception cref="MinScoreNotSpecifiedException">
    /// This exception is thrown when minimal score was not specified and request had score value.
    /// </exception>
    ValidationResult Validate(VerifyResponse response, string? action = null, float? score = null);
}
