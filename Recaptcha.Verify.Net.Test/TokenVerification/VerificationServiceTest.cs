using Recaptcha.Verify.Net.Exceptions.Configuration;
using Recaptcha.Verify.Net.Exceptions.Processing;
using Xunit;

namespace Recaptcha.Verify.Net.Test.TokenVerification;

public class VerificationServiceTest
{
    public static TheoryData<string?> EmptyStrings => new(null, string.Empty, "   ");

    public static TheoryData<string, float> GetResponseTokens()
    {
        var theoryData = new TheoryData<string, float>();

        foreach (var tokenScore in TokenVerificationFixture.ValidResponseTokens)
        {
            theoryData.Add(tokenScore.Key, tokenScore.Value);
        }

        return theoryData;
    }

    [Theory]
    [MemberData(nameof(EmptyStrings))]
    public async Task Verify_MissingSecretKey_Throws(string? secretKey)
    {
        var verificationService = VerificationServiceFixture.Create(secretKey!);

        await Assert.ThrowsAsync<SecretKeyNotSpecifiedException>(() => verificationService.VerifyAsync(TokenVerificationFixture.ValidResponseTokens.First().Key));
    }

    [Theory]
    [MemberData(nameof(EmptyStrings))]
    public async Task Verify_EmptyResponse_Throws(string? response)
    {
        var verificationService = VerificationServiceFixture.Create(TokenVerificationFixture.ValidSecretKey);

        await Assert.ThrowsAsync<EmptyCaptchaAnswerException>(() => verificationService.VerifyAsync(response!));
    }

    [Fact]
    public async Task Verify_ClientException_Throws()
    {
        var verificationService = VerificationServiceFixture.Create(TokenVerificationFixture.ValidSecretKey, true);

        await Assert.ThrowsAsync<VerifyRequestException>(() => verificationService.VerifyAsync(TokenVerificationFixture.ValidResponseTokens.First().Key));
    }

    [Fact]
    public async Task Verify_InvalidResponseToken_ReturnsVerificationResult()
    {
        var verificationService = VerificationServiceFixture.Create(TokenVerificationFixture.ValidSecretKey);

        var verificationResult = await verificationService.VerifyAsync(TokenVerificationFixture.InvalidResponseToken);

        Assert.NotNull(verificationResult);
        Assert.False(verificationResult.Success);
    }

    [Theory]
    [MemberData(nameof(GetResponseTokens))]
    public async Task Verify_ValidResponseToken_ReturnsVerificationResult(string responseToken, float score)
    {
        var verificationService = VerificationServiceFixture.Create(TokenVerificationFixture.ValidSecretKey);

        var verificationResult = await verificationService.VerifyAsync(responseToken);

        Assert.NotNull(verificationResult);
        Assert.True(verificationResult.Success);
        Assert.Equal(score, verificationResult.Score);
    }
}
