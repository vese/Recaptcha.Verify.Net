using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Recaptcha.Verify.Net.VerificationResultValidation;

/// <inheritdoc />
/// <summary>
/// Recaptcha verification result validation service constructor.
/// </summary>
/// <param name="options">Recaptcha validation options.</param>
/// <param name="logger">Logger.</param>
internal class RecaptchaVerificationResultValidationService(IOptions<RecaptchaValidationOptions> options, ILogger<RecaptchaVerificationResultValidationService> logger) : IRecaptchaVerificationResultValidationService
{
    private readonly RecaptchaValidationOptions options = options.Value;

    /// <inheritdoc />
    public ValidationResult Validate(VerifyResponse response, string? action = null, float? score = null)
    {
        var validationResult = new ValidationResult
        {
            ResponseSuccessful = response.Success,
            IsV3 = response.IsV3,
            ActionMatches = false,
            ScoreSatisfies = false,
        };

        if (response.Success && response.IsV3)
        {
            var expectedAction = GetExpectedAction(action);

            validationResult.ActionMatches = expectedAction.Equals(response.Action);

            var scoreThreshold = GetScoreThreshold(score, expectedAction);

            validationResult.ScoreSatisfies = response.Score!.Value >= scoreThreshold;

            logger.ResponseChecked(expectedAction, scoreThreshold, validationResult);
        }
        else
        {
            logger.ResponseChecked(null, null, validationResult);
        }

        return validationResult;
    }

    private string GetExpectedAction(string? action)
    {
        if (!string.IsNullOrWhiteSpace(action))
        {
            return action;
        }

        if (!string.IsNullOrWhiteSpace(options.Action))
        {
            return options.Action;
        }

        throw new EmptyActionException();
    }

    private float GetScoreThreshold(float? score, string action)
    {
        if (score.HasValue)
        {
            return score.Value;
        }

        if (options.ActionsScoreThresholds is not null && options.ActionsScoreThresholds.TryGetValue(action, out var scoreThreshold))
        {
            return scoreThreshold;
        }

        if (options.ScoreThreshold.HasValue)
        {
            return options.ScoreThreshold.Value;
        }

        throw new MinScoreNotSpecifiedException(action);
    }
}
