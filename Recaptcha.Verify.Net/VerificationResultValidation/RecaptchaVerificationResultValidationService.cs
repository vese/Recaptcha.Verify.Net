using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.Exceptions.Configuration;
using Recaptcha.Verify.Net.Logging;
using Recaptcha.Verify.Net.TokenVerification.Client.Models.Response;
using Recaptcha.Verify.Net.VerificationResultValidation.Models;

namespace Recaptcha.Verify.Net.VerificationResultValidation;

/// <inheritdoc />
/// <summary>
/// Recaptcha service constructor.
/// </summary>
/// <param name="recaptchaOptions">Recaptcha options.</param>
/// <param name="logger">Logger.</param>
public class RecaptchaVerificationResultValidationService(IOptions<RecaptchaOptions> recaptchaOptions, ILogger<RecaptchaVerificationResultValidationService> logger) : IRecaptchaVerificationResultValidationService
{
    private readonly RecaptchaOptions _recaptchaOptions = recaptchaOptions.Value;

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

        if (!string.IsNullOrWhiteSpace(_recaptchaOptions.Action))
        {
            return _recaptchaOptions.Action;
        }

        throw new EmptyActionException();
    }

    private float GetScoreThreshold(float? score, string action)
    {
        if (score.HasValue)
        {
            return score.Value;
        }

        if (_recaptchaOptions.ActionsScoreThresholds is not null && _recaptchaOptions.ActionsScoreThresholds.TryGetValue(action, out var scoreThreshold))
        {
            return scoreThreshold;
        }

        if (_recaptchaOptions is not null && _recaptchaOptions.ScoreThreshold.HasValue)
        {
            return _recaptchaOptions.ScoreThreshold.Value;
        }

        throw new MinScoreNotSpecifiedException(action);
    }
}
