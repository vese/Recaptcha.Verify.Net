using Microsoft.AspNetCore.Mvc.Filters;

namespace Recaptcha.Verify.Net.Configuration;

/// <summary>
/// Options for configuring token extractors that retrieve reCAPTCHA response tokens from requests.
/// </summary>
public class RecaptchaTokenExtractorOptions
{
    /// <summary>
    /// Name of the request header that contains the reCAPTCHA response token.
    /// </summary>
    public string? Header { get; set; }

    /// <summary>
    /// Name of the form field that contains the reCAPTCHA response token.
    /// </summary>
    public string? Form { get; set; }

    /// <summary>
    /// Name of the query parameter that contains the reCAPTCHA response token.
    /// </summary>
    [Obsolete("Do not pass token in query parameters. It is not secure.")]
    public string? Query { get; set; }

    /// <summary>
    /// Name of the action argument that contains the reCAPTCHA response token.
    /// Action arguments are the mapped arguments of the controller method.
    /// </summary>
    public string? ActionArgument { get; set; }

    /// <summary>
    /// Delegate for getting reCAPTCHA response token from action arguments.
    /// Actions arguments are mapped arguments of controller method.
    /// </summary>
    public Func<IDictionary<string, object?>, string?>? GetResponseTokenFromActionArguments { get; set; }

    /// <summary>
    /// Delegate for getting reCAPTCHA response token from executing context.
    /// </summary>
    public Func<ActionExecutingContext, string?>? GetResponseTokenFromExecutingContext { get; set; }
}
