using Microsoft.AspNetCore.Mvc.Filters;

namespace Recaptcha.Verify.Net.TokenExtraction;

/// <summary>
/// Service for extracting reCAPTCHA response token from the request using registered token extractors.
/// <para>Uses first-wins strategy: returns the first non-empty token found.</para>
/// </summary>
/// <exception cref="Exceptions.Configuration.TokenExtractorNotFound">
/// This exception is thrown when no <see cref="IRecaptchaTokenExtractor"/> implementation is registered in DI.
/// </exception>
/// <exception cref="Exceptions.Processing.EmptyCaptchaAnswerException">
/// This exception is thrown when all registered extractors returned null or empty token.
/// </exception>
internal class RecaptchaTokenExtractionService(IEnumerable<IRecaptchaTokenExtractor> tokenExtractors) : IRecaptchaTokenExtractionService
{
    /// <inheritdoc />
    public string GetToken(ActionExecutingContext context)
    {
        string? recaptchaToken = null;
        var recaptchaTokenExtracted = false;
        var tokenExtractorsCount = 0;

        foreach (var tokenExtractor in tokenExtractors)
        {
            tokenExtractorsCount++;

            recaptchaToken = tokenExtractor.GetToken(context);

            if (!string.IsNullOrWhiteSpace(recaptchaToken))
            {
                recaptchaTokenExtracted = true;
                break;
            }
        }

        if (tokenExtractorsCount == 0)
        {
            throw new TokenExtractorNotFound();
        }

        if (!recaptchaTokenExtracted)
        {
            throw new EmptyCaptchaAnswerException();
        }

        return recaptchaToken!;
    }
}
