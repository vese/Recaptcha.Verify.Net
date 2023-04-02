using Microsoft.Extensions.Logging;
using Recaptcha.Verify.Net.Attribute;
using System.Collections.Generic;
using System.Threading;

namespace Recaptcha.Verify.Net.Configuration
{
    /// <summary>
    /// Recaptcha options.
    /// </summary>
    public class RecaptchaOptions
    {
        /// <summary>
        /// The shared key between your site and reCAPTCHA.
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// Optional. Action to check for V3 Recaptcha request.
        /// <para>Action specified in <see cref="RecaptchaService.VerifyAndCheckAsync(string, string, float, CancellationToken)"/>
        /// or in <see cref="RecaptchaAttribute"/> will be used instead of this value.</para>
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Optional. Score threshold for V3 Recaptcha (0.0 - 1.0).
        /// <para>Score threshold specified in <see cref="RecaptchaService.VerifyAndCheckAsync(string, string, float, CancellationToken)"/>
        /// or in <see cref="RecaptchaAttribute"/> will be used instead of this value.</para>
        /// </summary>
        public float? ScoreThreshold { get; set; }

        /// <summary>
        /// Optional. Map of actions score thresholds for V3 Recaptcha.
        /// <para>Score threshold specified in <see cref="RecaptchaService.VerifyAndCheckAsync(string, string, float, CancellationToken)"/>
        /// or in <see cref="RecaptchaAttribute"/> will be used instead of this value.</para>
        /// </summary>
        public Dictionary<string, float> ActionsScoreThresholds { get; set; }

        /// <summary>
        /// Default returning message for unsuccessful validation and checking.
        /// <para>
        /// This message would be replaced by value processed by <see cref="RecaptchaAttributeOptions.OnVerificationFailed"/>, 
        /// <see cref="RecaptchaAttributeOptions.OnRecaptchaServiceException"/>, <see cref="RecaptchaAttributeOptions.OnException"/>
        /// or <see cref="RecaptchaAttributeOptions.OnReturnBadRequest"/>.
        /// </para>
        /// </summary>
        public string VerificationFailedMessage { get; set; } = "Recaptcha verification failed";

        /// <summary>
        /// Options for <see cref="RecaptchaAttribute"/>.
        /// </summary>
        public RecaptchaAttributeOptions AttributeOptions { get; set; } = new RecaptchaAttributeOptions();

        /// <summary>
        /// When <c>True</c> logs events such as "request sent" with <see cref="LogLevel.Information"/> using <see cref="ILogger"/> registered in di container.
        /// Default is <c>True</c>.
        /// </summary>
        public bool EnableLogging { get; set; } = true;

        /// <summary>
        /// When <c>True</c> logs exceptions with <see cref="LogLevel.Error"/> using <see cref="ILogger"/> registered in di container.
        /// Default is <c>True</c>.
        /// </summary>
        public bool EnableExceptionLogging { get; set; } = true;
    }
}
