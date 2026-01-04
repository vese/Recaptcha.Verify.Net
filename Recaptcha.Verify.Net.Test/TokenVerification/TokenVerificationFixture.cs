namespace Recaptcha.Verify.Net.Test.TokenVerification;

internal static class TokenVerificationFixture
{
    public const string InvalidSecretKey = "InvalidSecretKey";
    public const string ValidSecretKey = "ValidSecretKey";
    public const string InvalidResponseToken = "ResponseTokenInvalid";
    public static readonly IReadOnlyDictionary<string, float> ValidResponseTokens = new Dictionary<string, float>
    {
        { "ResponseToken1", 1.0f },
        { "ResponseToken2", 0.5f },
        { "ResponseToken3", 0.2f },
        { "ResponseToken4", 0.0f }
    };
}
