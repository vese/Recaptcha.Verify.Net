using Recaptcha.Verify.Net.Exceptions;
using Recaptcha.Verify.Net.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Recaptcha.Verify.Net
{
    /// <summary>
    /// Service for verifying reCAPTCHA response token.
    /// </summary>
    public interface IRecaptchaService
    {
        /// <summary>
        /// Map of actions score thresholds for V3 Recaptcha.
        /// </summary>
        Dictionary<string, float> ActionsScoreThresholds { get; }

        /// <summary>
        /// Verifies reCAPTCHA response token and checks score (for v3) and action.
        /// https://developers.google.com/recaptcha/docs/verify#api-request
        /// <para>Takes score threshold from options <see cref="RecaptchaOptions"/>.</para>
        /// </summary>
        /// <param name="request">Verify reCAPTCHA response token request params.</param>
        /// <param name="action">Action that the action from the response should be equal to.</param>
        /// <param name="cancellationToken"></param>
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
        /// <exception cref="HttpRequestException">
        /// This exception is thrown when http request failed.
        /// </exception>
        Task<CheckResult> VerifyAndCheckAsync(VerifyRequest request, string action,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies reCAPTCHA response token and checks score (for v3) and action.
        /// https://developers.google.com/recaptcha/docs/verify#api-request
        /// <para>Takes score threshold from options <see cref="RecaptchaOptions"/>.</para>
        /// </summary>
        /// <param name="response">The user response token provided by the reCAPTCHA client-side integration on your site.</param>
        /// <param name="action">Action that the action from the response should be equal to.</param>
        /// <param name="cancellationToken"></param>
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
        /// <exception cref="HttpRequestException">
        /// This exception is thrown when http request failed.
        /// </exception>
        Task<CheckResult> VerifyAndCheckAsync(string response, string action,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies reCAPTCHA response token and checks score (for v3) and action.
        /// https://developers.google.com/recaptcha/docs/verify#api-request
        /// </summary>
        /// <param name="request">Verify reCAPTCHA response token request params.</param>
        /// <param name="action">Action that the action from the response should be equal to.</param>
        /// <param name="score">Score threshold.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.
        /// The task result contains result of check <see cref="CheckResult"/>.</returns>
        /// <exception cref="EmptyCaptchaAnswerException">
        /// This exception is thrown when captcha answer is empty.
        /// </exception>
        /// <exception cref="SecretKeyNotSpecifiedException">
        /// This exception is thrown when secret key was not specified in options or request params.
        /// </exception>
        /// <exception cref="HttpRequestException">
        /// This exception is thrown when http request failed.
        /// </exception>
        Task<CheckResult> VerifyAndCheckAsync(VerifyRequest request, string action,
            float score, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies reCAPTCHA response token and checks score (for v3) and action.
        /// https://developers.google.com/recaptcha/docs/verify#api-request
        /// </summary>
        /// <param name="response">The user response token provided by the reCAPTCHA client-side integration on your site.</param>
        /// <param name="action">Action that the action from the response should be equal to.</param>
        /// <param name="score">Score threshold.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.
        /// The task result contains result of check <see cref="CheckResult"/>.</returns>
        /// <exception cref="EmptyCaptchaAnswerException">
        /// This exception is thrown when captcha answer is empty.
        /// </exception>
        /// <exception cref="SecretKeyNotSpecifiedException">
        /// This exception is thrown when secret key was not specified in options or request params.
        /// </exception>
        /// <exception cref="HttpRequestException">
        /// This exception is thrown when http request failed.
        /// </exception>
        Task<CheckResult> VerifyAndCheckAsync(string response, string action,
            float score, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies reCAPTCHA response token.
        /// https://developers.google.com/recaptcha/docs/verify#api-request
        /// </summary>
        /// <param name="request">Verify reCAPTCHA response token request params.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.
        /// The task result contains verification response <see cref="VerifyResponse"/>.</returns>
        /// <exception cref="EmptyCaptchaAnswerException">
        /// This exception is thrown when captcha answer is empty.
        /// </exception>
        /// <exception cref="SecretKeyNotSpecifiedException">
        /// This exception is thrown when secret key was not specified in options or request params.
        /// </exception>
        /// <exception cref="HttpRequestException">
        /// This exception is thrown when http request failed.
        /// </exception>
        Task<VerifyResponse> VerifyAsync(VerifyRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies reCAPTCHA response token.
        /// https://developers.google.com/recaptcha/docs/verify#api-request
        /// </summary>
        /// <param name="response">The user response token provided by the reCAPTCHA client-side integration on your site.</param>
        /// <param name="secret">The shared key between your site and reCAPTCHA.
        /// This parameter could be unsetted if <see cref="RecaptchaOptions"/> was configured.</param>
        /// <param name="remoteIp">The user's IP address.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.
        /// The task result contains verification response <see cref="VerifyResponse"/>.</returns>
        /// <exception cref="EmptyCaptchaAnswerException">
        /// This exception is thrown when captcha answer is empty.
        /// </exception>
        /// <exception cref="SecretKeyNotSpecifiedException">
        /// This exception is thrown when secret key was not specified in options or request params.
        /// </exception>
        /// <exception cref="HttpRequestException">
        /// This exception is thrown when http request failed.
        /// </exception>
        Task<VerifyResponse> VerifyAsync(string response, string secret = null,
            string remoteIp = null, CancellationToken cancellationToken = default);
    }
}
