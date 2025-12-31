using Microsoft.AspNetCore.Mvc.Filters;

namespace Recaptcha.Verify.Net.TokenExtraction;

internal class ExecutingContextTokenExtractor(Func<ActionExecutingContext, string?> getToken) : IRecaptchaTokenExtractor
{
    public string? GetToken(ActionExecutingContext context) => getToken(context);
}
