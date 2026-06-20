using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.TokenVerification;

namespace Recaptcha.Verify.Net.Test.TokenVerification;

internal static class VerificationServiceFixture
{
    public static IRecaptchaVerificationService Create(string secretKey, bool clientThrowException = false)
    {
        var verificationOptions = new RecaptchaVerificationOptions
        {
            SecretKey = secretKey
        };

        return new RecaptchaVerificationService(
            Options.Create(verificationOptions),
            RecaptchaClientFixture.Create(clientThrowException),
            NullLoggerFactory.Instance.CreateLogger<RecaptchaVerificationService>());
    }
}
