using Microsoft.Extensions.Options;
using Recaptcha.Verify.Net.Exceptions;
using Recaptcha.Verify.Net.Models;
using Refit;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Recaptcha.Verify.Net
{
    /// <inheritdoc />
    public class RecaptchaService : IRecaptchaService
    {
        private readonly RecaptchaOptions _recaptchaOptions;
        private readonly IRecaptchaClient _recaptchaClient;

        /// <inheritdoc />
        public Dictionary<string, float> ActionsScoreThresholds => _recaptchaOptions.ActionsScoreThresholds;

        /// <summary>
        /// Recaptcha service constructor.
        /// </summary>
        /// <param name="recaptchaOptions">Recaptcha options.</param>
        /// <param name="recaptchaClient">Recaptcha Refit client.</param>
        public RecaptchaService(IOptions<RecaptchaOptions> recaptchaOptions, IRecaptchaClient recaptchaClient)
        {
            _recaptchaOptions = recaptchaOptions?.Value;
            _recaptchaClient = recaptchaClient;
        }

        /// <inheritdoc />
        public async Task<CheckResult> VerifyAndCheckAsync(VerifyRequest request, string action,
            CancellationToken cancellationToken = default)
        {
            var response = await VerifyAsync(request, cancellationToken);

            var checkResult = new CheckResult()
            {
                Response = response,
                ActionMatches = response.Success && action.Equals(response.Action)
            };

            if (response.Success && response.Score.HasValue)
            {
                if (_recaptchaOptions == null)
                {
                    throw new MinScoreNotSpecifiedException();
                }

                if (_recaptchaOptions.ActionsScoreThresholds != null &&
                    _recaptchaOptions.ActionsScoreThresholds.TryGetValue(action, out var scoreThreshold))
                {
                    checkResult.ScoreSatisfies = response.Score.Value >= scoreThreshold;
                    return checkResult;
                }

                if (_recaptchaOptions == null || !_recaptchaOptions.ScoreThreshold.HasValue)
                {
                    throw new MinScoreNotSpecifiedException();
                }

                checkResult.ScoreSatisfies = response.Score.Value >= _recaptchaOptions.ScoreThreshold.Value;
            }

            return checkResult;
        }

        /// <inheritdoc />
        public Task<CheckResult> VerifyAndCheckAsync(string response, string action,
            CancellationToken cancellationToken = default) => VerifyAndCheckAsync(
                new VerifyRequest()
                {
                    Response = response,
                },
                action, cancellationToken);

        /// <inheritdoc />
        public async Task<CheckResult> VerifyAndCheckAsync(VerifyRequest request, string action, float score,
            CancellationToken cancellationToken = default)
        {
            var response = await VerifyAsync(request, cancellationToken);

            return new CheckResult()
            {
                Response = response,
                ActionMatches = response.Success && response.Action.Equals(action),
                ScoreSatisfies = response.Success && response.Score.HasValue && response.Score.Value > score
            };
        }

        /// <inheritdoc />
        public Task<CheckResult> VerifyAndCheckAsync(string response, string action,
            float score, CancellationToken cancellationToken = default) => VerifyAndCheckAsync(
                new VerifyRequest()
                {
                    Response = response,
                },
                action, score, cancellationToken);

        /// <inheritdoc />
        public async Task<VerifyResponse> VerifyAsync(VerifyRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Response))
            {
                throw new EmptyCaptchaAnswerException();
            }

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
                return await _recaptchaClient.VerifyAsync(request, cancellationToken);
            }
            catch (ApiException e)
            {
                throw new HttpRequestException(e);
            }
        }

        /// <inheritdoc />
        public Task<VerifyResponse> VerifyAsync(string response, string secret = null,
            string remoteIp = null, CancellationToken cancellationToken = default) =>
            VerifyAsync(
                new VerifyRequest()
                {
                    Response = response,
                    Secret = secret,
                    RemoteIp = remoteIp
                },
                cancellationToken);
    }
}
