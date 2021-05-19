using Newtonsoft.Json;

namespace Recaptcha.Verify.Net.Models.Response
{
    public class VerifyResponseV3 : VerifyResponse
    {
        /// <summary>
        /// The score for this request (0.0 - 1.0).
        /// </summary>
        [JsonProperty("score")]
        public double Score { get; set; }

        // The action name for this request (important to verify).
        [JsonProperty("action")]
        public string Action { get; set; }
    }
}
