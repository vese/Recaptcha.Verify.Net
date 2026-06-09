using Microsoft.AspNetCore.Mvc.Filters;

namespace Recaptcha.Verify.Net.TokenExtraction;

/// <summary>
/// Service for extracting reCAPTCHA response token from the request using registered token extractors.
/// </summary>
public interface IRecaptchaTokenExtractionService
{
    /// <summary>
    /// Extracts reCAPTCHA response token from the <see cref="ActionExecutingContext"/>.
    /// </summary>
    /// <param name="context">A context for action filters.</param>
    /// <returns>Extracted reCAPTCHA response token.</returns>
    /// <exception cref="Exceptions.Processing.EmptyCaptchaAnswerException">
    /// This exception is thrown when the extracted token is null or empty.
    /// </exception>
    /// <exception cref="Exceptions.Configuration.RecaptchaServiceConfigurationException"/>
    /// <exception cref="Exceptions.Processing.RecaptchaServiceProcessingException"/>
    string GetToken(ActionExecutingContext context);
}
