using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.Exceptions;
using Refit;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Recaptcha.Verify.Net
{
    /// <inheritdoc />
    public class RecaptchaService : IRecaptchaService
    {
        private readonly RecaptchaOptions _recaptchaOptions;
        private readonly IRecaptchaClient _recaptchaClient;
        private readonly ILogger _logger;

        /// <summary>
        /// Recaptcha service constructor.
        /// </summary>
        /// <param name="recaptchaOptions">Recaptcha options.</param>
        /// <param name="recaptchaClient">Recaptcha Refit client.</param>
        /// <param name="loggerFactory">Optional. Logger factory.</param>
        public RecaptchaService(IOptions<RecaptchaOptions> recaptchaOptions, IRecaptchaClient recaptchaClient, ILoggerFactory loggerFactory = null)
        {
            _recaptchaOptions = recaptchaOptions?.Value;
            _recaptchaClient = recaptchaClient;
            _logger = loggerFactory?.CreateLogger<RecaptchaService>();
        }

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
        public Task<VerifyResponse> VerifyAsync(string response, string secret = null, string remoteIp = null, CancellationToken cancellationToken = default) =>
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
                    throw LogException(new SecretKeyNotSpecifiedException());
                }

                request.Secret = _recaptchaOptions.SecretKey;
            }

            if (string.IsNullOrWhiteSpace(request.Response))
            {
                throw LogException(new EmptyCaptchaAnswerException());
            }

            try
            {
                LogInfo("Sending verify request. Response token: {token}, remote IP: {ip}.", request.Response, request.RemoteIp);
                var result = await _recaptchaClient.VerifyAsync(request, cancellationToken);
                LogInfo("Verify request succeeded. Response: {response}.", JsonSerializer.Serialize(result));
                return result;
            }
            catch (ApiException e)
            {
                throw LogException(new HttpRequestException(e));
            }
        }

        private async Task<CheckResult> VerifyAndCheckCoreAsync(VerifyRequest request, string action, float? score, CancellationToken cancellationToken)
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
                    throw LogException(new EmptyActionException());
                }

                checkResult.ActionMatches = response.Success && actionToCheck.Equals(response.Action);

                float scoreThreshold;
                if (score.HasValue)
                {
                    scoreThreshold = score.Value;
                }
                else if (_recaptchaOptions?.ActionsScoreThresholds != null && _recaptchaOptions.ActionsScoreThresholds.TryGetValue(action, out scoreThreshold))
                {
                }
                else if (_recaptchaOptions != null && _recaptchaOptions.ScoreThreshold.HasValue)
                {
                    scoreThreshold = _recaptchaOptions.ScoreThreshold.Value;
                }
                else
                {
                    throw LogException(new MinScoreNotSpecifiedException(actionToCheck));
                }

                checkResult.ScoreSatisfies = response.Score.Value >= scoreThreshold;
            }

            LogInfo("Verify response checked. Result: {checkResult}.", JsonSerializer.Serialize(checkResult));
            return checkResult;
        }

        private void LogInfo(string message, params object[] args)
        {
            if (_recaptchaOptions.EnableLogging && _logger != null)
            {
                _logger.LogInformation(message, args);
            }
        }

        private T LogException<T>(T e) where T : Exception
        {
            if (_recaptchaOptions.EnableExceptionLogging && _logger != null)
            {
                _logger.LogError(e, e.Message);
            }
            return e;
        }
    }
}
