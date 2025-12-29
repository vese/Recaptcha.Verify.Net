using Microsoft.AspNetCore.Mvc.Filters;
using Recaptcha.Verify.Net.Configuration;

namespace Recaptcha.Verify.Net.Attribute;

internal static class AttributeExtensions
{
    internal static string? GetResponseToken(this ActionExecutingContext context, RecaptchaAttributeOptions options) =>
        context.GetTokenFromActionArguments(options) ??
        context.GetTokenFromExecutingContext(options) ??
        context.GetTokenFromHeader(options) ??
        context.GetTokenFromQuery(options) ??
        context.GetTokenFromForm(options);

    private static string? GetTokenFromActionArguments(this ActionExecutingContext context, RecaptchaAttributeOptions options) =>
        options.GetResponseTokenFromActionArguments?.Invoke(context.ActionArguments);

    private static string? GetTokenFromExecutingContext(this ActionExecutingContext context, RecaptchaAttributeOptions options) =>
        options.GetResponseTokenFromExecutingContext?.Invoke(context);

    private static string? GetTokenFromHeader(this ActionExecutingContext context, RecaptchaAttributeOptions options)
    {
        if (string.IsNullOrEmpty(options.ResponseTokenNameInHeader))
        {
            return null;
        }

        if (context.HttpContext.Request.Headers.TryGetValue(options.ResponseTokenNameInHeader, out var tokens))
        {
            return tokens.FirstOrDefault();
        }

        return null;
    }

    private static string? GetTokenFromQuery(this ActionExecutingContext context, RecaptchaAttributeOptions options)
    {
        if (string.IsNullOrEmpty(options.ResponseTokenNameInQuery))
        {
            return null;
        }

        if (context.HttpContext.Request.Query.TryGetValue(options.ResponseTokenNameInQuery, out var tokens))
        {
            return tokens.FirstOrDefault();
        }

        return null;
    }

    private static string? GetTokenFromForm(this ActionExecutingContext context, RecaptchaAttributeOptions options)
    {
        if (string.IsNullOrEmpty(options.ResponseTokenNameInForm))
        {
            return null;
        }

        if (!context.HttpContext.Request.HasFormContentType)
        {
            return null;
        }

        if (context.HttpContext.Request.Form.TryGetValue(options.ResponseTokenNameInForm, out var tokens))
        {
            return tokens.FirstOrDefault();
        }

        return null;
    }
}
