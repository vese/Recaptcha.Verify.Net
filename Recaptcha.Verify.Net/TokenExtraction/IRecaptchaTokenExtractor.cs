using Microsoft.AspNetCore.Mvc.Filters;

namespace Recaptcha.Verify.Net.TokenExtraction;

/// <summary>
/// Service for extracting extract
/// </summary>
public interface IRecaptchaTokenExtractor
{
    /// <summary>
    /// Extracts reCAPTCHA token from <see cref="ActionExecutingContext"/>
    /// </summary>
    /// <param name="context">A context for action filters</param>
    /// <returns>Extracted token</returns>
    string? GetToken(ActionExecutingContext context);
}
