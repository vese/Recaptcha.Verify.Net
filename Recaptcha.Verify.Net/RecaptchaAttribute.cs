using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Recaptcha.Verify.Net;

/// <summary>
/// Verifies reCAPTCHA response token and checks score (for v3) and action.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RecaptchaAttribute : ActionFilterAttribute
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
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var recaptchaOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<RecaptchaOptions>>().Value;

        try
        {
            var result = await ProcessRecaptchaAsync(context, recaptchaOptions);

            if (result is not null)
            {
                context.Result = result;
                return;
            }
        }
        catch (Exception e) when (e is not RecaptchaServiceException)
        {
            if (recaptchaOptions.AttributeOptions.OnException is not null || recaptchaOptions.AttributeOptions.OnRecaptchaServiceException is not null || recaptchaOptions.AttributeOptions.OnReturnBadRequest is not null)
            {
                context.Result =
                    recaptchaOptions.AttributeOptions.OnReturnBadRequest?.Invoke(context, _action, null, null, e) ??
                    recaptchaOptions.AttributeOptions.OnException?.Invoke(context, _action, null, e) ??
                    new BadRequestObjectResult(recaptchaOptions.VerificationFailedMessage);
                return;
            }

            throw new RecaptchaUnknownException(e);
        }
        catch (RecaptchaServiceException e) when (recaptchaOptions.AttributeOptions.OnException is not null || recaptchaOptions.AttributeOptions.OnRecaptchaServiceException is not null || recaptchaOptions.AttributeOptions.OnReturnBadRequest is not null)
        {
            context.Result =
                recaptchaOptions.AttributeOptions.OnReturnBadRequest?.Invoke(context, _action, null, e, null) ??
                recaptchaOptions.AttributeOptions.OnRecaptchaServiceException?.Invoke(context, _action, null, e) ??
                new BadRequestObjectResult(recaptchaOptions.VerificationFailedMessage);
            return;
        }

        await base.OnActionExecutionAsync(context, next);
    }

    private async Task<IActionResult?> ProcessRecaptchaAsync(ActionExecutingContext context, RecaptchaOptions recaptchaOptions)
    {
        var recaptchaToken = GetRecaptchaToken(context);

        var verificationService = context.HttpContext.RequestServices.GetRequiredService<IRecaptchaVerificationService>();
        var validationService = context.HttpContext.RequestServices.GetRequiredService<IRecaptchaVerificationResultValidationService>();

        var remoteIp = context.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        var cancellationToken = recaptchaOptions.AttributeOptions.UseCancellationToken ?
            context.HttpContext.RequestAborted : CancellationToken.None;

        var verifyRequest = new VerifyRequest()
        {
            Secret = recaptchaOptions.SecretKey,
            Response = recaptchaToken,
            RemoteIp = remoteIp
        };

        var verifyResponse = await verificationService.VerifyAsync(verifyRequest, cancellationToken);

        var validationResult = validationService.Validate(verifyResponse, _action, _score);

        if (!validationResult.Success)
        {
            return recaptchaOptions.AttributeOptions.OnReturnBadRequest?.Invoke(context, _action, validationResult, null, null) ??
                recaptchaOptions.AttributeOptions.OnVerificationFailed?.Invoke(context, _action, validationResult) ??
                new BadRequestObjectResult(recaptchaOptions.VerificationFailedMessage);
        }

        return null;
    }

    private static string GetRecaptchaToken(ActionExecutingContext context)
    {
        var tokenExtractors = context.HttpContext.RequestServices.GetServices<IRecaptchaTokenExtractor>();

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
