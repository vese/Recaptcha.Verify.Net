using Recaptcha.Verify.Net.Exceptions;
using System.Linq;
using Xunit;

namespace Recaptcha.Verify.Net.Test.Tests;

public class VerifyAndCheckV3Test : BaseRecaptchaServiceTest
{
    public VerifyAndCheckV3Test() : base(true) { }

    [Theory]
    [MemberData(nameof(NoSecretKeyData))]
    public async void VerifyAndCheck_NoSecretKey(string? secretKey, string? token)
    {
        var recaptchaService = CreateService(secretKey);
        await Assert.ThrowsAsync<SecretKeyNotSpecifiedException>(
            () => recaptchaService.VerifyAndCheckAsync(token, RecaptchaServiceFixture.Action));
    }

    [Theory]
    [MemberData(nameof(NoResponseTokenData))]
    public async void VerifyAndCheck_NoResponseToken(string secretKey, string? token)
    {
        var recaptchaService = CreateService(secretKey);
        await Assert.ThrowsAsync<EmptyCaptchaAnswerException>(
            () => recaptchaService.VerifyAndCheckAsync(token, string.Empty));
        await Assert.ThrowsAsync<EmptyCaptchaAnswerException>(
            () => recaptchaService.VerifyAndCheckAsync(token, RecaptchaServiceFixture.Action));
    }

    [Theory]
    [MemberData(nameof(InvalidSecretKeyData))]
    public async void VerifyAndCheck_InvalidSecretKey(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var recaptchaService = CreateService(secretKey);
        var response = await recaptchaService.VerifyAndCheckAsync(tokenFixture.Token, tokenFixture.Action);
        Assert.False(response.Success);
    }

    [Theory]
    [MemberData(nameof(InvalidItemsData))]
    public async void VerifyAndCheck_ValidSecretKey_InvalidToken(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var recaptchaService = CreateService(secretKey);
        var response = await recaptchaService.VerifyAndCheckAsync(tokenFixture.Token, tokenFixture.Action);
        Assert.False(response.Response.Success);
        Assert.False(response.Success);
    }

    [Theory]
    [MemberData(nameof(InvalidItemsWithUnknownErrorKeyData))]
    public async void VerifyAndCheck_ValidSecretKey_InvalidToken_UnknownErrorKey(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var recaptchaService = CreateService(secretKey);
        var response = await recaptchaService.VerifyAndCheckAsync(tokenFixture.Token, tokenFixture.Action);
        Assert.False(response.Response.Success);
        Assert.False(response.Success);
        var e = Assert.Throws<UnknownErrorKeyException>(() => response.Response.Errors);
        Assert.Equal(tokenFixture.Errors?.First(), e.Key);
    }

    [Theory]
    [MemberData(nameof(ValidItemsData))]
    public async void VerifyAndCheck_ValidSecretKey_ValidToken_NoAction(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var recaptchaService = CreateService(secretKey);
        await Assert.ThrowsAsync<EmptyActionException>(
            () => recaptchaService.VerifyAndCheckAsync(tokenFixture.Token));
        foreach (var emptyAction in EmptyStrings)
        {
            await Assert.ThrowsAsync<EmptyActionException>(
                () => recaptchaService.VerifyAndCheckAsync(tokenFixture.Token, emptyAction));
        }
    }

    [Theory]
    [MemberData(nameof(ValidItemsData))]
    public async void VerifyAndCheck_ValidSecretKey_ValidToken_WithAction(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var recaptchaService = CreateService(secretKey);
        var noScoreException = await Assert.ThrowsAsync<MinScoreNotSpecifiedException>(
            () => recaptchaService.VerifyAndCheckAsync(tokenFixture.Token, tokenFixture.Action));
        Assert.Equal(tokenFixture.Action, noScoreException.Action);

        recaptchaService = CreateService(secretKey, null, null, tokenFixture.Action);
        noScoreException = await Assert.ThrowsAsync<MinScoreNotSpecifiedException>(
            () => recaptchaService.VerifyAndCheckAsync(tokenFixture.Token));
        Assert.Equal(tokenFixture.Action, noScoreException.Action);
    }

    [Theory]
    [MemberData(nameof(ValidItemsData))]
    public async void VerifyAndCheck_ValidSecretKey_ValidToken_NoScore(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var recaptchaService = CreateService(secretKey);
        var noScoreException = await Assert.ThrowsAsync<MinScoreNotSpecifiedException>(
            () => recaptchaService.VerifyAndCheckAsync(tokenFixture.Token, tokenFixture.Action));
        Assert.Equal(tokenFixture.Action, noScoreException.Action);
    }

    [Theory]
    [MemberData(nameof(ValidItemsData))]
    public async void VerifyAndCheck_ValidSecretKey_ValidToken_WithScore(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var recaptchaService = CreateService(secretKey, RecaptchaServiceFixture.Score);
        var response = await recaptchaService.VerifyAndCheckAsync(tokenFixture.Token, tokenFixture.Action);
        var success = tokenFixture.Score.HasValue && tokenFixture.Score.Value >= RecaptchaServiceFixture.Score;
        Assert.Equal(success, response.Success);
    }

    [Theory]
    [MemberData(nameof(ValidItemsData))]
    public async void VerifyAndCheck_ValidSecretKey_ValidToken_WithScoreMap(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var recaptchaService = CreateService(secretKey, null, RecaptchaServiceFixture.ScoreMap);
        var response = await recaptchaService.VerifyAndCheckAsync(tokenFixture.Token, tokenFixture.Action);
        var success = tokenFixture.Score.HasValue &&
            RecaptchaServiceFixture.ScoreMap.TryGetValue(tokenFixture.Action!, out var score) &&
            tokenFixture.Score.Value >= score;
        Assert.Equal(success, response.Success);
    }

    [Theory]
    [MemberData(nameof(ValidItemData))]
    public async void VerifyAndCheck_ValidSecretKey_ValidToken_WithScoreMap_MissingValue(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var recaptchaService = CreateService(secretKey, null, new());
        var e = await Assert.ThrowsAsync<MinScoreNotSpecifiedException>(
            () => recaptchaService.VerifyAndCheckAsync(tokenFixture.Token, tokenFixture.Action));
        Assert.Equal(tokenFixture.Action, e.Action);
    }

    [Theory]
    [MemberData(nameof(ValidItemsData))]
    public async void VerifyAndCheck_ValidSecretKey_ValidToken_WithScoreAndScoreMap(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var recaptchaService = CreateService(secretKey, RecaptchaServiceFixture.Score, RecaptchaServiceFixture.ScoreMap);
        var response = await recaptchaService.VerifyAndCheckAsync(tokenFixture.Token, tokenFixture.Action);
        var success = tokenFixture.Score.HasValue &&
            (RecaptchaServiceFixture.ScoreMap.TryGetValue(tokenFixture.Action!, out var score) ?
                tokenFixture.Score.Value >= score :
                tokenFixture.Score.Value >= RecaptchaServiceFixture.Score);
        Assert.Equal(success, response.Success);
    }

    [Theory]
    [MemberData(nameof(ValidItemsData))]
    public async void VerifyAndCheck_ValidSecretKey_ValidToken_WithScore_Directly(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var recaptchaService = CreateService(secretKey);
        var response = await recaptchaService.VerifyAndCheckAsync(
            tokenFixture.Token, tokenFixture.Action, RecaptchaServiceFixture.Score);
        var success = tokenFixture.Score.HasValue && tokenFixture.Score.Value >= RecaptchaServiceFixture.Score;
        Assert.Equal(success, response.Success);
    }

    [Theory]
    [MemberData(nameof(ValidItemsData))]
    public async void VerifyAndCheck_ValidSecretKey_ValidToken_WithScore_DirectlyOverInOptions(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var recaptchaService = CreateService(secretKey, RecaptchaServiceFixture.Score, RecaptchaServiceFixture.ScoreMap);
        var response = await recaptchaService.VerifyAndCheckAsync(
            tokenFixture.Token, tokenFixture.Action, RecaptchaServiceFixture.ScoreUnsuccessful);
        Assert.False(response.Success);
    }
}