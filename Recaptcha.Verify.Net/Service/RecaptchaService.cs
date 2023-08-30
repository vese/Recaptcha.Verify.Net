using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.Exceptions;
using Recaptcha.Verify.Net.Logging;
using Refit;
using System;
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
        /// <param name="logger">Logger.</param>
        public RecaptchaService(IOptions<RecaptchaOptions> recaptchaOptions, IRecaptchaClient recaptchaClient, ILogger<RecaptchaService> logger)
        {
            _recaptchaOptions = recaptchaOptions?.Value;
            _recaptchaClient = recaptchaClient;
            _logger = logger;
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
                    throw Log(new SecretKeyNotSpecifiedException(), _logger.MissingSecretKeyError);
                }

                request.Secret = _recaptchaOptions.SecretKey;
            }

            if (string.IsNullOrWhiteSpace(request.Response))
            {
                throw Log(new EmptyCaptchaAnswerException(), _logger.MissingCaptchaAnswerError);
            }

            try
            {
                _logger.SendingRequest(request);
                var result = await _recaptchaClient.VerifyAsync(request, cancellationToken);
                _logger.RequestCompleted(result);
                return result;
            }
            catch (ApiException e)
            {
                throw Log(new HttpRequestException(e), _logger.HttpRequestError);
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
                    throw Log(new EmptyActionException(), _logger.MissingActionError);
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
                    throw Log(new MinScoreNotSpecifiedException(actionToCheck), _logger.MissingMinScoreError);
                }

                checkResult.ScoreSatisfies = response.Score.Value >= scoreThreshold;

                _logger.ResponseChecked(actionToCheck, scoreThreshold, checkResult);
            }
            else
            {
                _logger.ResponseChecked(null, null, checkResult);
            }

            return checkResult;
        }

        private static T Log<T>(T e, Action<T> action) where T : Exception
        {
            action.Invoke(e);
            return e;
        }
    }
}
