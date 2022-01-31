using Microsoft.Extensions.Options;
using Recaptcha.Verify.Net.Exceptions;
using Recaptcha.Verify.Net.Models;
using Recaptcha.Verify.Net.Models.Request;
using Recaptcha.Verify.Net.Models.Response;
using System.Threading;
using System.Threading.Tasks;

namespace Recaptcha.Verify.Net
{
    /// <summary>
    /// Service for verifying reCAPTCHA response token.
    /// </summary>
    public class RecaptchaService : IRecaptchaService
    {
        private readonly RecaptchaOptions _recaptchaOptions;
        private readonly IRecaptchaClient _recaptchaClient;

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

        /// <summary>
        /// Verifies reCAPTCHA response token and checks score (for v3) and action.
        /// https://developers.google.com/recaptcha/docs/verify#api-request
        /// <para>Takes score threshold from options <see cref="RecaptchaOptions"/>.</para>
        /// </summary>
        /// <param name="request">Verify reCAPTCHA response token request params.</param>
        /// <param name="action">Action that the action from the response should be equal to.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.
        /// The task result contains result of check <see cref="CheckResult"/>.</returns>
        /// <exception cref="EmptyCaptchaAnswerException">
        /// This exception is thrown when captcha answer is empty.
        /// </exception>
        /// <exception cref="SecretKeyNotSpecifiedException">
        /// This exception is thrown when secret key was not specified in options or request params.
        /// </exception>
        /// <exception cref="MinScoreNotSpecifiedException">
        /// This exception is thrown when minimal score was not specified and request had score value.
        /// </exception>
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
                if (_recaptchaOptions == null || !_recaptchaOptions.ScoreThreshold.HasValue)
                {
                    throw new MinScoreNotSpecifiedException();
                }

                checkResult.ScoreSatisfies = response.Score.Value > _recaptchaOptions.ScoreThreshold.Value;
            }

            return checkResult;
        }

        /// <summary>
        /// Verifies reCAPTCHA response token and checks score (for v3) and action.
        /// https://developers.google.com/recaptcha/docs/verify#api-request
        /// <para>Takes score threshold from options <see cref="RecaptchaOptions"/>.</para>
        /// </summary>
        /// <param name="response">The user response token provided by the reCAPTCHA client-side integration on your site.</param>
        /// <param name="action">Action that the action from the response should be equal to.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.
        /// The task result contains result of check <see cref="CheckResult"/>.</returns>
        /// <exception cref="EmptyCaptchaAnswerException">
        /// This exception is thrown when captcha answer is empty.
        /// </exception>
        /// <exception cref="SecretKeyNotSpecifiedException">
        /// This exception is thrown when secret key was not specified in options or request params.
        /// </exception>
        /// <exception cref="MinScoreNotSpecifiedException">
        /// This exception is thrown when minimal score was not specified and request had score value.
        /// </exception>
        public Task<CheckResult> VerifyAndCheckAsync(string response, string action,
            CancellationToken cancellationToken = default) => VerifyAndCheckAsync(
                new VerifyRequest()
                {
                    Response = response,
                },
                action, cancellationToken);

        /// <summary>
        /// Verifies reCAPTCHA response token and checks score (for v3) and action.
        /// https://developers.google.com/recaptcha/docs/verify#api-request
        /// </summary>
        /// <param name="request">Verify reCAPTCHA response token request params.</param>
        /// <param name="action">Action that the action from the response should be equal to.</param>
        /// <param name="score">Score threshold.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.
        /// The task result contains result of check <see cref="CheckResult"/>.</returns>
        /// <exception cref="EmptyCaptchaAnswerException">
        /// This exception is thrown when captcha answer is empty.
        /// </exception>
        /// <exception cref="SecretKeyNotSpecifiedException">
        /// This exception is thrown when secret key was not specified in options or request params.
        /// </exception>
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

        /// <summary>
        /// Verifies reCAPTCHA response token and checks score (for v3) and action.
        /// https://developers.google.com/recaptcha/docs/verify#api-request
        /// </summary>
        /// <param name="response">The user response token provided by the reCAPTCHA client-side integration on your site.</param>
        /// <param name="action">Action that the action from the response should be equal to.</param>
        /// <param name="score">Score threshold.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.
        /// The task result contains result of check <see cref="CheckResult"/>.</returns>
        /// <exception cref="EmptyCaptchaAnswerException">
        /// This exception is thrown when captcha answer is empty.
        /// </exception>
        /// <exception cref="SecretKeyNotSpecifiedException">
        /// This exception is thrown when secret key was not specified in options or request params.
        /// </exception>
        public Task<CheckResult> VerifyAndCheckAsync(string response, string action,
            float score, CancellationToken cancellationToken = default) => VerifyAndCheckAsync(
                new VerifyRequest()
                {
                    Response = response,
                },
                action, score, cancellationToken);

        /// <summary>
        /// Verifies reCAPTCHA response token.
        /// https://developers.google.com/recaptcha/docs/verify#api-request
        /// </summary>
        /// <param name="request">Verify reCAPTCHA response token request params.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.
        /// The task result contains verification response <see cref="VerifyResponse"/>.</returns>
        /// <exception cref="EmptyCaptchaAnswerException">
        /// This exception is thrown when captcha answer is empty.
        /// </exception>
        /// <exception cref="SecretKeyNotSpecifiedException">
        /// This exception is thrown when secret key was not specified in options or request params.
        /// </exception>
        public Task<VerifyResponse> VerifyAsync(VerifyRequest request, CancellationToken cancellationToken = default)
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

            return _recaptchaClient.VerifyAsync(request, cancellationToken);
        }

        /// <summary>
        /// Verifies reCAPTCHA response token.
        /// https://developers.google.com/recaptcha/docs/verify#api-request
        /// </summary>
        /// <param name="response">The user response token provided by the reCAPTCHA client-side integration on your site.</param>
        /// <param name="secret">The shared key between your site and reCAPTCHA.
        /// This parameter could be unsetted if <see cref="RecaptchaOptions"/> was configured.</param>
        /// <param name="remoteIp">The user's IP address.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.
        /// The task result contains verification response <see cref="VerifyResponse"/>.</returns>
        /// <exception cref="EmptyCaptchaAnswerException">
        /// This exception is thrown when captcha answer is empty.
        /// </exception>
        /// <exception cref="SecretKeyNotSpecifiedException">
        /// This exception is thrown when secret key was not specified in options or request params.
        /// </exception>
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
