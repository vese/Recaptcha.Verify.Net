using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.VerificationResultValidation;

namespace Recaptcha.Verify.Net.Test.VerificationResultValidation;

internal static class ValidationServiceFixture
{
    public static IRecaptchaVerificationResultValidationService Create(string? action, float? scoreThreshold,
        IReadOnlyDictionary<string, float>? actionsScoreThresholds)
    {
        var recaptchaOptions = new RecaptchaOptions
        {
            Action = action,
            ScoreThreshold = scoreThreshold,
            ActionsScoreThresholds = actionsScoreThresholds
        };

        return new RecaptchaVerificationResultValidationService(
            Options.Create(recaptchaOptions),
            NullLoggerFactory.Instance.CreateLogger<RecaptchaVerificationResultValidationService>());
    }
}
