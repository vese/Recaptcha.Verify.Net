using Microsoft.AspNetCore.Mvc.Filters;

namespace Recaptcha.Verify.Net.TokenExtraction;

internal class FormTokenExtractor(string parameterName) : IRecaptchaTokenExtractor
{
    public string? GetToken(ActionExecutingContext context)
    {
        if (string.IsNullOrEmpty(parameterName))
        {
            return null;
        }

        if (!context.HttpContext.Request.HasFormContentType)
        {
            return null;
        }

        if (context.HttpContext.Request.Form.TryGetValue(parameterName, out var tokens))
        {
            return tokens.FirstOrDefault();
        }

        return null;
    }
}
