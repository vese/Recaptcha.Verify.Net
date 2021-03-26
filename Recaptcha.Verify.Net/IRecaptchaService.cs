using Recaptcha.Verify.Net.Models.Request;
using Recaptcha.Verify.Net.Models.Response;
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
        /// Verifies reCAPTCHA response token.
        /// https://developers.google.com/recaptcha/docs/verify#api-request
        /// </summary>
        /// <param name="request">Verify reCAPTCHA response token request params.</param>
        /// <returns>A <see cref="VerifyResponse"/> verification response.</returns>
        Task<VerifyResponse> VerifyAsync(VerifyRequest request, CancellationToken cancellationToken = default);
    }
}
