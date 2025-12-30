using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recaptcha.Verify.Net.Client.Models.Request;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.Exceptions;
using Recaptcha.Verify.Net.Exceptions.Configuration;
using Recaptcha.Verify.Net.Exceptions.Processing;
using Recaptcha.Verify.Net.Logging;
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
        var tokenExtractors = context.HttpContext.RequestServices.GetServices<ITokenExtractor>();

        if (!tokenExtractors.Any())
        {
            throw new TokenExtractorNotFound();
        }

        CheckResult? checkResult = null;

        try
        {
            string? recaptchaToken = null;
            var recaptchaTokenExtracted = false;

            foreach (var tokenExtractor in tokenExtractors)
            {
                recaptchaToken = tokenExtractor.GetToken(context);

                if (!string.IsNullOrWhiteSpace(recaptchaToken))
                {
                    recaptchaTokenExtracted = true;
                    break;
                }
            }

            if (!recaptchaTokenExtracted)
            {
                var e = new EmptyCaptchaAnswerException();

                var logger = context.HttpContext.RequestServices.GetService<ILogger<RecaptchaAttribute>>();

                if (logger is not null)
                {
                    e.WithLog(logger.MissingCaptchaAnswerError);
                }

                var recaptchaOptions2 = context.HttpContext.RequestServices.GetRequiredService<IOptions<RecaptchaOptions>>().Value;

                HandleBadResult(context, recaptchaOptions2, checkResult, e, null);

                return;
            }

            var recaptchaService = context.HttpContext.RequestServices.GetRequiredService<IRecaptchaService>();
            var recaptchaOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<RecaptchaOptions>>().Value;


            var remoteIp = context.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            var cancellationToken = recaptchaOptions.AttributeOptions.UseCancellationToken ?
                context.HttpContext.RequestAborted : CancellationToken.None;

            if (_score.HasValue)
            {
                checkResult = await recaptchaService.VerifyAndCheckAsync(
                    new VerifyRequest()
                    {
                        Secret = recaptchaOptions.SecretKey,
                        Response = recaptchaToken!,
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
                        Response = recaptchaToken!,
                        RemoteIp = remoteIp
                    },
                    _action,
                    cancellationToken);
            }
        }
        catch (RecaptchaServiceException e)
        {
            var recaptchaOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<RecaptchaOptions>>().Value;

            HandleBadResult(context, recaptchaOptions, checkResult, e, null);
            return;
        }
        catch (Exception e)
        {
            var recaptchaOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<RecaptchaOptions>>().Value;

            HandleBadResult(context, recaptchaOptions, checkResult, null, e);
            return;
        }

        if (!checkResult.Success)
        {
            var recaptchaOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<RecaptchaOptions>>().Value;

            HandleBadResult(context, recaptchaOptions, checkResult, null, null);
            return;
        }

        await base.OnActionExecutionAsync(context, next);
    }

    private void HandleBadResult(ActionExecutingContext context, RecaptchaOptions recaptchaOptions, CheckResult? result, RecaptchaServiceException? re, Exception? e)
    {
        var options = recaptchaOptions.AttributeOptions;
        IActionResult? handleResult = null;

        if (re != null)
        {
            handleResult = options.OnRecaptchaServiceException?.Invoke(context, _action, result, re);
        }
        else if (e != null)
        {
            handleResult = options.OnException?.Invoke(context, _action, result, e);
        }
        else if (result != null)
        {
            handleResult = options.OnVerificationFailed?.Invoke(context, _action, result);
        }

        if (options.OnReturnBadRequest != null)
        {
            handleResult = options.OnReturnBadRequest?.Invoke(context, _action, result, re, e);
        }

        context.Result = handleResult ?? new BadRequestObjectResult(recaptchaOptions.VerificationFailedMessage);
    }
}
