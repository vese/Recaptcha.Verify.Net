using Recaptcha.Verify.Net.TokenVerification.Client.Models.Response;

namespace Recaptcha.Verify.Net.Test.VerificationResultValidation;

internal static class VerificationResultValidationFixture
{
    public const string Action = "Action";
    public const string Action2 = "Action2";
    public const float Score = 0.5f;
    public const float Score2 = 0.2f;
    public static readonly IReadOnlyDictionary<string, float> ActionsScoresEmpty = new Dictionary<string, float>().AsReadOnly();
    public static readonly IReadOnlyDictionary<string, float> ActionsScores = new Dictionary<string, float>
    {
        { Action, Score }
    }.AsReadOnly();
    public static readonly IReadOnlyDictionary<string, float> ActionsScores2 = new Dictionary<string, float>
    {
        { Action2, Score2 }
    }.AsReadOnly();

    public static readonly VerifyResponse UnsuccessfulVerificationResult = new()
    {
        Success = false,
        Score = null
    };

    public static readonly VerifyResponse V2SuccessfulVerificationResult = new()
    {
        Success = true,
        Score = null
    };

    public static readonly VerifyResponse[] SuccessfulVerificationResults =
    [
        new()
        {
            Success = true,
            Score = 1.0f,
            Action = Action
        },
        new()
        {
            Success = true,
            Score = 0.5f,
            Action = Action
        },
        new()
        {
            Success = true,
            Score = 0.2f,
            Action = Action
        },
        new()
        {
            Success = true,
            Score = 0.0f,
            Action = Action
        }
    ];

    public static readonly VerifyResponse[] VerificationResults =
    [
        UnsuccessfulVerificationResult,
        .. SuccessfulVerificationResults
    ];
}
