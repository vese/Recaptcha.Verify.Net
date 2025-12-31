using Microsoft.AspNetCore.Mvc.Filters;

namespace Recaptcha.Verify.Net.TokenExtraction;

internal class ActionArgumentsTokenExtractor : IRecaptchaTokenExtractor
{
    private readonly Func<IDictionary<string, object?>, string?>? _getToken;
    private readonly string? _argumentName;

    public ActionArgumentsTokenExtractor(Func<IDictionary<string, object?>, string?> getToken)
    {
        _getToken = getToken;
    }

    public ActionArgumentsTokenExtractor(string argumentName)
    {
        _argumentName = argumentName;
    }

    public string? GetToken(ActionExecutingContext context)
    {
        if (_getToken is not null)
        {
            return _getToken(context.ActionArguments);
        }

        if (context.ActionArguments.TryGetValue(_argumentName!, out var token) && token is string)
        {
            return (string?)token;
        }

        return null;
    }
}
