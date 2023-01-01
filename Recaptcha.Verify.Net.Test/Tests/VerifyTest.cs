using Recaptcha.Verify.Net.Exceptions;
using System;
using System.Linq;
using Xunit;

namespace Recaptcha.Verify.Net.Test.Tests;

public class VerifyTest : BaseRecaptchaServiceTest
{
    [Theory]
    [MemberData(nameof(NoSecretKeyData))]
    public async void Verify_NoSecretKey(string? secretKey, string? token)
    {
        var recaptchaService = CreateService();
        await Assert.ThrowsAsync<SecretKeyNotSpecifiedException>(
            () => recaptchaService.VerifyAsync(token));

        recaptchaService = CreateService(secretKey);
        await Assert.ThrowsAsync<SecretKeyNotSpecifiedException>(
            () => recaptchaService.VerifyAsync(token));

        recaptchaService = CreateService();
        await Assert.ThrowsAsync<SecretKeyNotSpecifiedException>(
            () => recaptchaService.VerifyAsync(token, secretKey));
    }

    [Theory]
    [MemberData(nameof(NoResponseTokenData))]
    public async void Verify_NoResponseToken(string secretKey, string? token)
    {
        var recaptchaService = CreateService(secretKey);
        await Assert.ThrowsAsync<EmptyCaptchaAnswerException>(
            () => recaptchaService.VerifyAsync(token));
    }

    [Theory]
    [MemberData(nameof(InvalidSecretKeyData))]
    public async void Verify_InvalidSecretKey(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var recaptchaService = CreateService(secretKey);
        var response = await recaptchaService.VerifyAsync(tokenFixture.Token);
        Assert.False(response.Success);

        recaptchaService = CreateService();
        response = await recaptchaService.VerifyAsync(tokenFixture.Token, secretKey);
        Assert.False(response.Success);
    }

    [Theory]
    [MemberData(nameof(ValidItemsData))]
    public async void Verify_ValidSecretKey_ValidToken(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var recaptchaService = CreateService(secretKey);
        var response = await recaptchaService.VerifyAsync(tokenFixture.Token);
        Assert.True(response.Success);

        recaptchaService = CreateService();
        response = await recaptchaService.VerifyAsync(tokenFixture.Token, secretKey);
        Assert.True(response.Success);
    }

    [Theory]
    [MemberData(nameof(InvalidItemsData))]
    public async void Verify_ValidSecretKey_InvalidToken(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var assertErrors = tokenFixture.ErrorsItems
            .Select(e => (Action<VerifyError>)(item => Assert.Equal(e, item)))
            .ToArray();

        var recaptchaService = CreateService(secretKey);
        var response = await recaptchaService.VerifyAsync(tokenFixture.Token);
        Assert.False(response.Success);
        Assert.Collection(response.Errors, assertErrors);

        recaptchaService = CreateService();
        response = await recaptchaService.VerifyAsync(tokenFixture.Token, secretKey);
        Assert.False(response.Success);
        Assert.Collection(response.Errors, assertErrors);
    }

    [Theory]
    [MemberData(nameof(InvalidItemsWithUnknownErrorKeyData))]
    public async void Verify_ValidSecretKey_InvalidToken_UnknownErrorKey(string secretKey, ResponseTokenFixture tokenFixture)
    {
        var recaptchaService = CreateService(secretKey);
        var response = await recaptchaService.VerifyAsync(tokenFixture.Token);
        Assert.False(response.Success);
        var e = Assert.Throws<UnknownErrorKeyException>(() => response.Errors);
        Assert.Equal(tokenFixture.Errors?.First(), e.Key);

        recaptchaService = CreateService();
        response = await recaptchaService.VerifyAsync(tokenFixture.Token, secretKey);
        Assert.False(response.Success);
        e = Assert.Throws<UnknownErrorKeyException>(() => response.Errors);
        Assert.Equal(tokenFixture.Errors?.First(), e.Key);
    }
}