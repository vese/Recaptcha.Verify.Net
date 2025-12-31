using Microsoft.AspNetCore.Mvc.Filters;

namespace Recaptcha.Verify.Net.TokenExtraction;

[Obsolete("Do not pass token in query parameters. It is not secure.")]
internal class QueryTokenExtractor(string parameterName) : IRecaptchaTokenExtractor
{
    public string? GetToken(ActionExecutingContext context)
    {
        if (string.IsNullOrEmpty(parameterName))
        {
            return null;
        }

        if (context.HttpContext.Request.Query.TryGetValue(parameterName, out var tokens))
        {
            return tokens.FirstOrDefault();
        }

        return null;
    }
}
