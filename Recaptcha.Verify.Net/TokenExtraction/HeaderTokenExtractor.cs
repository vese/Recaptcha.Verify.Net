using Microsoft.AspNetCore.Mvc.Filters;

namespace Recaptcha.Verify.Net.TokenExtraction;

internal class HeaderTokenExtractor(string headerName) : IRecaptchaTokenExtractor
{
    public string? GetToken(ActionExecutingContext context)
    {
        if (string.IsNullOrEmpty(headerName))
        {
            return null;
        }

        if (context.HttpContext.Request.Headers.TryGetValue(headerName, out var tokens))
        {
            return tokens.FirstOrDefault();
        }

        return null;
    }
}
