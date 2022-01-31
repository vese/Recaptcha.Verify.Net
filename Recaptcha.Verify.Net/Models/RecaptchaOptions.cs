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
        /// Optional. Minimal score for V3 Recaptcha (0.0 - 1.0).
        /// <para>Score threshold could be passed directly 
        /// into <see cref="RecaptchaService.VerifyAndCheckAsync(string, string, float, CancellationToken)"/>.</para>
        /// </summary>
        public float? ScoreThreshold { get; set; }

        /// <summary>
        /// Optional. Map of actions minimal score for V3 Recaptcha.
        /// </summary>
        public Dictionary<string, float> ActionsScoreThresholds { get; set; }
    }
}
