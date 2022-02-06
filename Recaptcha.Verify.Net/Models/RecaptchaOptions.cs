using System.Collections.Generic;
using System.Threading;

namespace Recaptcha.Verify.Net.Models
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
        /// Optional. Score threshold for V3 Recaptcha (0.0 - 1.0).
        /// <para>Score threshold could be passed directly 
        /// into <see cref="RecaptchaService.VerifyAndCheckAsync(string, string, float, CancellationToken)"/>.</para>
        /// </summary>
        public float? ScoreThreshold { get; set; }

        /// <summary>
        /// Optional. Map of actions score thresholds for V3 Recaptcha.
        /// </summary>
        public Dictionary<string, float> ActionsScoreThresholds { get; set; }

        /// <summary>
        /// Default returning message for unseccessfull validation and checking.
        /// <para>
        /// This message would be replaced by value processed by <see cref="RecaptchaAttributeOptions.OnVerificationFailed"/>, 
        /// <see cref="RecaptchaAttributeOptions.OnRecaptchaServiceException"/>, <see cref="RecaptchaAttributeOptions.OnException"/>
        /// or <see cref="RecaptchaAttributeOptions.OnReturnBadRequest"/>.
        /// </para>
        /// </summary>
        public string VerificationFailedMessage { get; set; } = "Recaptcha verification failed";

        /// <summary>
        /// <see cref="RecaptchaAttribute"/> options.
        /// </summary>
        public RecaptchaAttributeOptions RecaptchaAttributeOptions { get; set; } = new RecaptchaAttributeOptions();
    }
}
