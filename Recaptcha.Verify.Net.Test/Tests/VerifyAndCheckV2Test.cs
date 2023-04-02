using Recaptcha.Verify.Net.Exceptions;
using System.Linq;
using Xunit;

namespace Recaptcha.Verify.Net.Test.Tests;

public class VerifyAndCheckV2Test : BaseRecaptchaServiceTest
{
    public VerifyAndCheckV2Test() : base(false) { }

    [Theory]
    [MemberData(nameof(NoSecretKeyData))]
    public async void VerifyAndCheck_NoSecretKey(string? secretKey, string? token)
    {
        var recaptchaService = CreateService(secretKey);
        await Assert.ThrowsAsync<SecretKeyNotSpecifiedException>(
            () => recaptchaService.VerifyAndCheckAsync(token));
    }

    [Theory]
    [MemberData(nameof(NoResponseTokenData))]
    public async void VerifyAndCheck_NoResponseToken(string secretKey, string? token)
    {
        var recaptchaService = CreateService(secretKey);
        await Assert.ThrowsAsync<EmptyCaptchaAnswerException>(
            () => recaptchaService.VerifyAndCheckAsync(token));
        await Assert.ThrowsAsync<EmptyCaptchaAnswerException>(
            () => recaptchaService.VerifyAndCheckAsync(token));
    }

    [Theory]
    [MemberData(nameof(InvalidSecretKeyData))]
    public async void VerifyAndCheck_InvalidSecretKey(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var recaptchaService = CreateService(secretKey);
        var response = await recaptchaService.VerifyAndCheckAsync(tokenFixture.Token);
        Assert.False(response.Success);
    }

    [Theory]
    [MemberData(nameof(ValidItemsData))]
    public async void VerifyAndCheck_ValidSecretKey_ValidToken(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var recaptchaService = CreateService(secretKey);
        var response = await recaptchaService.VerifyAndCheckAsync(tokenFixture.Token);
        Assert.True(response.Success);
    }

    [Theory]
    [MemberData(nameof(InvalidItemsData))]
    public async void VerifyAndCheck_ValidSecretKey_InvalidToken(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var recaptchaService = CreateService(secretKey);
        var response = await recaptchaService.VerifyAndCheckAsync(tokenFixture.Token);
        Assert.False(response.Response.Success);
        Assert.False(response.Success);
    }

    [Theory]
    [MemberData(nameof(InvalidItemsWithUnknownErrorKeyData))]
    public async void VerifyAndCheck_ValidSecretKey_InvalidToken_UnknownErrorKey(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var recaptchaService = CreateService(secretKey);
        var e = await Assert.ThrowsAsync<UnknownErrorKeyException>(async () =>
        {
            var response = await recaptchaService.VerifyAndCheckAsync(tokenFixture.Token);
            Assert.False(response.Response.Success);
            Assert.False(response.Success);
            var _ = response.Response.Errors;
        });
        Assert.Equal(tokenFixture.Errors?.First(), e.Key);
    }
}