using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Recaptcha.Verify.Net.Attribute;

/// <summary>
/// Verifies reCAPTCHA response token and checks score (for v3) and action.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RecaptchaAttribute : System.Attribute, IAsyncActionFilter
{
    private readonly string? _action;
    private readonly float? _score;

    /// <summary>
    /// Verifies reCAPTCHA response token  and validates verification result (for v2 or v3 (if specified in <see cref="RecaptchaOptions"/>)).
    /// </summary>
    public RecaptchaAttribute() { }

    /// <summary>
    /// Verifies reCAPTCHA response token and validates verification result (for v3).
    /// </summary>
    /// <param name="action">Action that the action from the response should be equal to.</param>
    public RecaptchaAttribute(string action)
    {
        _action = action;
    }

    /// <summary>
    /// Verifies reCAPTCHA response token and validates verification result (for v3).
    /// </summary>
    /// <param name="action">Action that the action from the response should be equal to.</param>
    /// <param name="score">Score threshold. This value will be used instead of values from options</param>
    public RecaptchaAttribute(string action, float score)
    {
        _action = action;
        _score = score;
    }

    /// <summary>
    /// Verifies reCAPTCHA response token and checks score (for v3) and action.
    /// </summary>
    /// <param name="context">A context for executing action.</param>
    /// <param name="next">A delegate that contains next action.</param>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        try
        {
            var options = context.HttpContext.RequestServices.GetRequiredService<IOptions<RecaptchaAttributeOptions>>().Value;

            var result = await ProcessRecaptchaAsync(context, options);

            if (result is not null)
            {
                context.Result = result;
                return;
            }
        }
        catch (Exception e) when (e is not RecaptchaServiceException)
        {
            throw new RecaptchaUnknownException(e);
        }

        await next();
    }

    private async Task<IActionResult?> ProcessRecaptchaAsync(ActionExecutingContext context, RecaptchaAttributeOptions options)
    {
        var tokenExtractionService = context.HttpContext.RequestServices.GetRequiredService<IRecaptchaTokenExtractionService>();
        var recaptchaToken = tokenExtractionService.GetToken(context);

        var verificationService = context.HttpContext.RequestServices.GetRequiredService<IRecaptchaVerificationService>();
        var validationService = context.HttpContext.RequestServices.GetRequiredService<IRecaptchaVerificationResultValidationService>();

        var remote = context.HttpContext.Connection.RemoteIpAddress;
        var remoteIp = remote is null ? null : (remote.IsIPv4MappedToIPv6 ? remote.MapToIPv4().ToString() : remote.ToString());
        var cancellationToken = options.UseCancellationToken ?
            context.HttpContext.RequestAborted : CancellationToken.None;

        var verifyResponse = await verificationService.VerifyAsync(recaptchaToken, remoteIp, cancellationToken);

        var validationResult = validationService.Validate(verifyResponse, _action, _score);

        if (!validationResult.Success)
        {
            return options.OnVerificationFailed?.Invoke(context, _action, validationResult) ??
                new BadRequestObjectResult(options.VerificationFailedMessage);
        }

        return null;
    }
}
