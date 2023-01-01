using Recaptcha.Verify.Net.Configuration;
using System.Text.Json.Serialization;

namespace Recaptcha.Verify.Net
{
    /// <summary>
    /// Verify reCAPTCHA response token request params.
    /// </summary>
    public class VerifyRequest
    {
        /// <summary>
        /// Required. The shared key between your site and reCAPTCHA.
        /// This parameter could be unspecified if <see cref="RecaptchaOptions"/> was configured.
        /// </summary>
        [JsonPropertyName("secret")]
        public string Secret { get; set; }

        /// <summary>
        /// Required. The user response token provided by the reCAPTCHA client-side integration on your site.
        /// </summary>
        [JsonPropertyName("response")]
        public string Response { get; set; }

        /// <summary>
        /// Optional. The user's IP address.
        /// </summary>
        [JsonPropertyName("remoteip")]
        public string RemoteIp { get; set; }
    }
}
