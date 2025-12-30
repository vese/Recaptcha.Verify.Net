using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recaptcha.Verify.Net.Client;
using Recaptcha.Verify.Net.Client.Models.Request;
using Recaptcha.Verify.Net.Client.Models.Response;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.Exceptions;
using Recaptcha.Verify.Net.Exceptions.Configuration;
using Recaptcha.Verify.Net.Exceptions.Processing;
using Recaptcha.Verify.Net.Logging;
using Recaptcha.Verify.Net.Service.Models;

namespace Recaptcha.Verify.Net.Service;

/// <inheritdoc />
/// <summary>
/// Recaptcha service constructor.
/// </summary>
/// <param name="recaptchaOptions">Recaptcha options.</param>
/// <param name="recaptchaClient">Recaptcha client.</param>
/// <param name="logger">Logger.</param>
public class RecaptchaService(IOptions<RecaptchaOptions> recaptchaOptions, IRecaptchaClient recaptchaClient, ILogger<RecaptchaService> logger) : IRecaptchaService
{
    private readonly RecaptchaOptions _recaptchaOptions = recaptchaOptions.Value;

    /// <inheritdoc />
    public Task<CheckResult> VerifyAndCheckAsync(string response, CancellationToken cancellationToken = default) =>
        VerifyAndCheckCoreAsync(new VerifyRequest() { Response = response }, null, null, cancellationToken);

    /// <inheritdoc />
    public Task<CheckResult> VerifyAndCheckAsync(string response, string action, CancellationToken cancellationToken = default) =>
        VerifyAndCheckCoreAsync(new VerifyRequest() { Response = response }, action, null, cancellationToken);

    /// <inheritdoc />
    public Task<CheckResult> VerifyAndCheckAsync(string response, string action, float score, CancellationToken cancellationToken = default) =>
        VerifyAndCheckCoreAsync(new VerifyRequest() { Response = response }, action, score, cancellationToken);

    /// <inheritdoc />
    public Task<CheckResult> VerifyAndCheckAsync(VerifyRequest request, CancellationToken cancellationToken = default) =>
        VerifyAndCheckCoreAsync(request, null, null, cancellationToken);

    /// <inheritdoc />
    public Task<CheckResult> VerifyAndCheckAsync(VerifyRequest request, string action, CancellationToken cancellationToken = default) =>
        VerifyAndCheckCoreAsync(request, action, null, cancellationToken);

    /// <inheritdoc />
    public Task<CheckResult> VerifyAndCheckAsync(VerifyRequest request, string action, float score, CancellationToken cancellationToken = default) =>
        VerifyAndCheckCoreAsync(request, action, score, cancellationToken);

    /// <inheritdoc />
    public Task<VerifyResponse> VerifyAsync(string response, string? secret = null, string? remoteIp = null, CancellationToken cancellationToken = default) =>
        VerifyAsync(
            new VerifyRequest()
            {
                Response = response,
                Secret = secret,
                RemoteIp = remoteIp
            },
            cancellationToken);

    /// <inheritdoc />
    public async Task<VerifyResponse> VerifyAsync(VerifyRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Secret))
        {
            if (string.IsNullOrWhiteSpace(_recaptchaOptions?.SecretKey))
            {
                throw new SecretKeyNotSpecifiedException();
            }

            request.Secret = _recaptchaOptions.SecretKey;
        }

        try
        {
            logger.SendingRequest(request);
            var result = await recaptchaClient.VerifyAsync(request, cancellationToken);
            logger.RequestCompleted(result);
            return result;
        }
        catch (Exception e)
        {
            throw new VerifyRequestException(e);
        }
    }

    private async Task<CheckResult> VerifyAndCheckCoreAsync(VerifyRequest request, string? action, float? score, CancellationToken cancellationToken)
    {
        var response = await VerifyAsync(request, cancellationToken);

        var checkResult = new CheckResult()
        {
            Response = response,
            ActionMatches = false,
            ScoreSatisfies = false,
        };

        if (response.Success && response.IsV3)
        {
            string actionToCheck;
            if (!string.IsNullOrWhiteSpace(action))
            {
                actionToCheck = action;
            }
            else if (!string.IsNullOrWhiteSpace(_recaptchaOptions?.Action))
            {
                actionToCheck = _recaptchaOptions.Action;
            }
            else
            {
                throw new EmptyActionException();
            }

            checkResult.ActionMatches = response.Success && actionToCheck.Equals(response.Action);

            float scoreThreshold;
            if (score.HasValue)
            {
                scoreThreshold = score.Value;
            }
            else if (_recaptchaOptions?.ActionsScoreThresholds != null && _recaptchaOptions.ActionsScoreThresholds.TryGetValue(actionToCheck, out scoreThreshold))
            {
            }
            else if (_recaptchaOptions != null && _recaptchaOptions.ScoreThreshold.HasValue)
            {
                scoreThreshold = _recaptchaOptions.ScoreThreshold.Value;
            }
            else
            {
                throw new MinScoreNotSpecifiedException(actionToCheck);
            }

            checkResult.ScoreSatisfies = response.Score!.Value >= scoreThreshold;

            logger.ResponseChecked(actionToCheck, scoreThreshold, checkResult);
        }
        else
        {
            logger.ResponseChecked(null, null, checkResult);
        }

        return checkResult;
    }
}
