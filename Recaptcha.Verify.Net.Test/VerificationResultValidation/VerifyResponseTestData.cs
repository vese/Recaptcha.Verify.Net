using Recaptcha.Verify.Net.TokenVerification.Client.Models.Response;
using Xunit.Abstractions;

namespace Recaptcha.Verify.Net.Test.VerificationResultValidation;

public class VerifyResponseTestData : VerifyResponse, IXunitSerializable
{
    public static VerifyResponseTestData Create(VerifyResponse item) => new()
    {
        Success = item.Success,
        Score = item.Score,
        Action = item.Action,
        ChallengeTs = item.ChallengeTs,
        Hostname = item.Hostname,
        ApkPackageName = item.ApkPackageName,
        ErrorCodes = item.ErrorCodes
    };

    public void Deserialize(IXunitSerializationInfo info)
    {
        Success = info.GetValue<bool>(nameof(Success));
        Score = info.GetValue<float?>(nameof(Score));
        Action = info.GetValue<string?>(nameof(Action));
        ChallengeTs = info.GetValue<DateTime>(nameof(ChallengeTs));
        Hostname = info.GetValue<string?>(nameof(Hostname));
        ApkPackageName = info.GetValue<string?>(nameof(ApkPackageName));
        ErrorCodes = info.GetValue<List<string>?>(nameof(ErrorCodes));
    }

    public void Serialize(IXunitSerializationInfo info)
    {
        info.AddValue(nameof(Success), Success);
        info.AddValue(nameof(Score), Score);
        info.AddValue(nameof(Action), Action);
        info.AddValue(nameof(ChallengeTs), ChallengeTs);
        info.AddValue(nameof(Hostname), Hostname);
        info.AddValue(nameof(ApkPackageName), ApkPackageName);
        info.AddValue(nameof(ErrorCodes), ErrorCodes);
    }
}
