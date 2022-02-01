using Newtonsoft.Json;

namespace Recaptcha.Verify.Net.Models
{
    /// <summary>
    /// Verify reCAPTCHA response token request params.
    /// </summary>
    public class VerifyRequest
    {
        /// <summary>
        /// Required. The shared key between your site and reCAPTCHA.
        /// This parameter could be unsetted if <see cref="RecaptchaOptions"/> was configured.
        /// </summary>
        [JsonProperty("secret")]
        public string Secret { get; set; }

        /// <summary>
        /// Required. The user response token provided by the reCAPTCHA client-side integration on your site.
        /// </summary>
        [JsonProperty("response")]
        public string Response { get; set; }

        /// <summary>
        /// Optional. The user's IP address.
        /// </summary>
        [JsonProperty("remoteip")]
        public string RemoteIp { get; set; }
    }
}
