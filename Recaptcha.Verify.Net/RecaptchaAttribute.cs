using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Recaptcha.Verify.Net.Client.Models.Request;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.Exceptions;
using Recaptcha.Verify.Net.Exceptions.Configuration;
using Recaptcha.Verify.Net.Exceptions.Processing;
using Recaptcha.Verify.Net.Service;
using Recaptcha.Verify.Net.Service.Models;
using Recaptcha.Verify.Net.TokenExtraction;

namespace Recaptcha.Verify.Net;

/// <summary>
/// Verifies reCAPTCHA response token and checks score (for v3) and action.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RecaptchaAttribute : ActionFilterAttribute
{
    private readonly string _action;
    private readonly float? _score;

    /// <summary>
    /// Verifies reCAPTCHA response token and checks score (for v3) and action.
    /// </summary>
    /// <param name="action">Action that the action from the response should be equal to.</param>
    public RecaptchaAttribute(string action)
    {
        _action = action;
    }

    /// <summary>
    /// Verifies reCAPTCHA response token and checks score (for v3) and action.
    /// </summary>
    /// <param name="action">Action that the action from the response should be equal to.</param>
    /// <param name="score">Score threshold for V3 reCAPTCHA. This value will be used instead of values from options</param>
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
        try
        {
            var result = await ProcessRecaptchaAsync(context);

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

        await base.OnActionExecutionAsync(context, next);
    }

    private async Task<IActionResult?> ProcessRecaptchaAsync(ActionExecutingContext context)
    {
        var recaptchaToken = GetRecaptchaToken(context);

        var recaptchaService = context.HttpContext.RequestServices.GetRequiredService<IRecaptchaService>();
        var recaptchaOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<RecaptchaOptions>>().Value;

        var remoteIp = context.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        var cancellationToken = recaptchaOptions.AttributeOptions.UseCancellationToken ?
            context.HttpContext.RequestAborted : CancellationToken.None;

        CheckResult checkResult;

        if (_score.HasValue)
        {
            checkResult = await recaptchaService.VerifyAndCheckAsync(
                new VerifyRequest()
                {
                    Secret = recaptchaOptions.SecretKey,
                    Response = recaptchaToken,
                    RemoteIp = remoteIp
                },
                _action,
                _score.Value,
                cancellationToken);
        }
        else
        {
            checkResult = await recaptchaService.VerifyAndCheckAsync(
                new VerifyRequest()
                {
                    Secret = recaptchaOptions.SecretKey,
                    Response = recaptchaToken,
                    RemoteIp = remoteIp
                },
                _action,
                cancellationToken);
        }

        if (!checkResult.Success)
        {
            return recaptchaOptions.AttributeOptions.OnVerificationFailed?.Invoke(context, _action, checkResult) ??
                new BadRequestObjectResult(recaptchaOptions.VerificationFailedMessage);
        }

        return null;
    }

    private static string GetRecaptchaToken(ActionExecutingContext context)
    {
        var tokenExtractors = context.HttpContext.RequestServices.GetServices<ITokenExtractor>();

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
