using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using Recaptcha.Verify.Net.Attribute;
using Recaptcha.Verify.Net.Exceptions;
using Recaptcha.Verify.Net.Exceptions.Configuration;
using Recaptcha.Verify.Net.Exceptions.Processing;
using Recaptcha.Verify.Net.TokenExtraction;
using Recaptcha.Verify.Net.TokenVerification;
using Recaptcha.Verify.Net.TokenVerification.Client.Models.Response;
using Recaptcha.Verify.Net.VerificationResultValidation;
using Recaptcha.Verify.Net.VerificationResultValidation.Models;
using Xunit;

namespace Recaptcha.Verify.Net.Test.Attribute;

public class AttributeTest
{
    public static TheoryData<bool> BooleanValues => new(false, true);

    public static TheoryData<bool, string?, float?> GetExecuteParameters()
    {
        var theoryData = new TheoryData<bool, string?, float?>();

        foreach (var useCancellationToken in new bool[] { true, false })
        {
            foreach (var action in new string?[] { null, RecaptchaAttributeFixture.Action })
            {
                foreach (var score in new float?[] { null, RecaptchaAttributeFixture.Score })
                {
                    theoryData.Add(useCancellationToken, action, score);
                }
            }
        }

        return theoryData;
    }

    [Fact]
    public async Task Execute_NoTokenExtractionService_ThrowsRecaptchaUnknownException()
    {
        var options = RecaptchaAttributeFixture.GetRecaptchaOptions();

        (var context, var next) = ActionExecutingContextFixture.CreateActionExecutingContext(options, null, null, null);

        var attribute = new RecaptchaAttribute();

        var thrown = await Assert.ThrowsAsync<RecaptchaUnknownException>(() => attribute.OnActionExecutionAsync(context, next.Object));
        Assert.IsType<InvalidOperationException>(thrown.InnerException);

        next.Verify(x => x.Invoke(), Times.Never);
    }

    [Fact]
    public async Task Execute_TokenExtractionServiceThrowsTokenExtractorNotFound_PropagatesException()
    {
        (var tokenExtractionService, var verificationService, var validationService) = RecaptchaAttributeFixture.CreateServices(
            null, extractionException: new TokenExtractorNotFound());

        var options = RecaptchaAttributeFixture.GetRecaptchaOptions();

        (var context, var next) = ActionExecutingContextFixture.CreateActionExecutingContext(
            options, tokenExtractionService.Object, verificationService.Object, validationService.Object);

        var attribute = new RecaptchaAttribute();

        await Assert.ThrowsAsync<TokenExtractorNotFound>(() => attribute.OnActionExecutionAsync(context, next.Object));

        next.Verify(x => x.Invoke(), Times.Never);
    }

    [Fact]
    public async Task Execute_TokenExtractionServiceThrowsEmptyCaptchaAnswer_PropagatesException()
    {
        (var tokenExtractionService, var verificationService, var validationService) = RecaptchaAttributeFixture.CreateServices(
            null, extractionException: new EmptyCaptchaAnswerException());

        var options = RecaptchaAttributeFixture.GetRecaptchaOptions();

        (var context, var next) = ActionExecutingContextFixture.CreateActionExecutingContext(
            options, tokenExtractionService.Object, verificationService.Object, validationService.Object);

        var attribute = new RecaptchaAttribute();

        await Assert.ThrowsAsync<EmptyCaptchaAnswerException>(() => attribute.OnActionExecutionAsync(context, next.Object));

        next.Verify(x => x.Invoke(), Times.Never);
    }

    [Theory]
    [MemberData(nameof(BooleanValues))]
    public async Task Execute_v2_Successful(bool useCancellationToken)
    {
        (var tokenExtractionService, var verificationService, var validationService) = RecaptchaAttributeFixture.CreateServices(
            RecaptchaAttributeFixture.Token,
            new VerifyResponse(),
            new ValidationResult
            {
                ResponseSuccessful = true,
                IsV3 = false
            });

        var options = RecaptchaAttributeFixture.GetRecaptchaOptions(useCancellationToken);

        (var context, var next) = ActionExecutingContextFixture.CreateActionExecutingContext(
            options, tokenExtractionService.Object, verificationService.Object, validationService.Object);

        var attribute = new RecaptchaAttribute();

        await attribute.OnActionExecutionAsync(context, next.Object);

        VerifyServicesCalls(tokenExtractionService, verificationService, validationService, true, useCancellationToken, null, null);

        next.Verify(x => x.Invoke(), Times.Once);

        Assert.Null(context.Result);
    }

    [Theory]
    [MemberData(nameof(GetExecuteParameters))]
    public async Task Execute_v3_Successful(bool useCancellationToken, string? action, float? score)
    {
        (var tokenExtractionService, var verificationService, var validationService) = RecaptchaAttributeFixture.CreateServices(
            RecaptchaAttributeFixture.Token,
            new VerifyResponse(),
            new ValidationResult
            {
                ResponseSuccessful = true,
                IsV3 = true,
                ScoreSatisfies = true,
                ActionMatches = true
            });

        var options = RecaptchaAttributeFixture.GetRecaptchaOptions(useCancellationToken);

        (var context, var next) = ActionExecutingContextFixture.CreateActionExecutingContext(
            options, tokenExtractionService.Object, verificationService.Object, validationService.Object);

        var attribute = score.HasValue ? new RecaptchaAttribute(action!, score.Value) : new RecaptchaAttribute(action!);

        await attribute.OnActionExecutionAsync(context, next.Object);

        VerifyServicesCalls(tokenExtractionService, verificationService, validationService, true, useCancellationToken, action, score);

        next.Verify(x => x.Invoke(), Times.Once);

        Assert.Null(context.Result);
    }

    [Theory]
    [MemberData(nameof(GetExecuteParameters))]
    public async Task Execute_Unuccessful_ReturnsBadRequest(bool useCancellationToken, string? action, float? score)
    {
        (var tokenExtractionService, var verificationService, var validationService) = RecaptchaAttributeFixture.CreateServices(
            RecaptchaAttributeFixture.Token,
            new VerifyResponse(),
            new ValidationResult
            {
                ResponseSuccessful = false,
                IsV3 = true
            });

        var options = RecaptchaAttributeFixture.GetRecaptchaOptions(useCancellationToken);

        (var context, var next) = ActionExecutingContextFixture.CreateActionExecutingContext(
            options, tokenExtractionService.Object, verificationService.Object, validationService.Object);

        var attribute = score.HasValue ? new RecaptchaAttribute(action!, score.Value) : new RecaptchaAttribute(action!);

        await attribute.OnActionExecutionAsync(context, next.Object);

        VerifyServicesCalls(tokenExtractionService, verificationService, validationService, true, useCancellationToken, action, score);

        next.Verify(x => x.Invoke(), Times.Never);

        Assert.NotNull(context.Result);
        Assert.True(context.Result is BadRequestObjectResult);
        Assert.Equal(StatusCodes.Status400BadRequest, (context.Result as BadRequestObjectResult)!.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetExecuteParameters))]
    public async Task Execute_VerificationThrows_PropagatesException(bool useCancellationToken, string? action, float? score)
    {
        var verificationException = new Exception();

        (var tokenExtractionService, var verificationService, var validationService) = RecaptchaAttributeFixture.CreateServices(
            RecaptchaAttributeFixture.Token,
            null,
            new ValidationResult
            {
                ResponseSuccessful = false,
                IsV3 = true
            },
            verificationException: verificationException);

        var options = RecaptchaAttributeFixture.GetRecaptchaOptions(useCancellationToken);

        (var context, var next) = ActionExecutingContextFixture.CreateActionExecutingContext(
            options, tokenExtractionService.Object, verificationService.Object, validationService.Object);

        var attribute = score.HasValue ? new RecaptchaAttribute(action!, score.Value) : new RecaptchaAttribute(action!);

        var thrown = await Assert.ThrowsAsync<RecaptchaUnknownException>(() => attribute.OnActionExecutionAsync(context, next.Object));
        Assert.Same(verificationException, thrown.InnerException);

        tokenExtractionService.Verify(x => x.GetToken(It.IsAny<ActionExecutingContext>()), Times.Once);

        next.Verify(x => x.Invoke(), Times.Never);
    }

    [Theory]
    [MemberData(nameof(GetExecuteParameters))]
    public async Task Execute_ValidationThrows_PropagatesException(bool useCancellationToken, string? action, float? score)
    {
        var validationException = new Exception();

        (var tokenExtractionService, var verificationService, var validationService) = RecaptchaAttributeFixture.CreateServices(
            RecaptchaAttributeFixture.Token,
            new VerifyResponse(),
            validationException: validationException);

        var options = RecaptchaAttributeFixture.GetRecaptchaOptions(useCancellationToken);

        (var context, var next) = ActionExecutingContextFixture.CreateActionExecutingContext(
            options, tokenExtractionService.Object, verificationService.Object, validationService.Object);

        var attribute = score.HasValue ? new RecaptchaAttribute(action!, score.Value) : new RecaptchaAttribute(action!);

        var thrown = await Assert.ThrowsAsync<RecaptchaUnknownException>(() => attribute.OnActionExecutionAsync(context, next.Object));
        Assert.Same(validationException, thrown.InnerException);

        tokenExtractionService.Verify(x => x.GetToken(It.IsAny<ActionExecutingContext>()), Times.Once);

        next.Verify(x => x.Invoke(), Times.Never);
    }

    [Fact]
    public async Task Execute_PureIPv6RemoteIp_IsPassedThroughUnchanged()
    {
        var ipv6 = System.Net.IPAddress.Parse("2001:db8::1");

        (var tokenExtractionService, var verificationService, var validationService) = RecaptchaAttributeFixture.CreateServices(
            RecaptchaAttributeFixture.Token,
            new VerifyResponse(),
            new ValidationResult
            {
                ResponseSuccessful = true,
                IsV3 = false
            });

        var options = RecaptchaAttributeFixture.GetRecaptchaOptions();

        (var context, var next) = ActionExecutingContextFixture.CreateActionExecutingContext(
            options, tokenExtractionService.Object, verificationService.Object, validationService.Object, ipv6);

        var attribute = new RecaptchaAttribute();

        await attribute.OnActionExecutionAsync(context, next.Object);

        verificationService.Verify(
            x => x.VerifyAsync(RecaptchaAttributeFixture.Token, ipv6.ToString(), It.IsAny<CancellationToken>()),
            Times.Once);

        next.Verify(x => x.Invoke(), Times.Once);
    }

    [Fact]
    public async Task Execute_IPv4MappedToIPv6RemoteIp_IsMappedToIPv4()
    {
        var mapped = System.Net.IPAddress.Parse("::ffff:1.2.3.4");

        (var tokenExtractionService, var verificationService, var validationService) = RecaptchaAttributeFixture.CreateServices(
            RecaptchaAttributeFixture.Token,
            new VerifyResponse(),
            new ValidationResult
            {
                ResponseSuccessful = true,
                IsV3 = false
            });

        var options = RecaptchaAttributeFixture.GetRecaptchaOptions();

        (var context, var next) = ActionExecutingContextFixture.CreateActionExecutingContext(
            options, tokenExtractionService.Object, verificationService.Object, validationService.Object, mapped);

        var attribute = new RecaptchaAttribute();

        await attribute.OnActionExecutionAsync(context, next.Object);

        verificationService.Verify(
            x => x.VerifyAsync(RecaptchaAttributeFixture.Token, "1.2.3.4", It.IsAny<CancellationToken>()),
            Times.Once);

        next.Verify(x => x.Invoke(), Times.Once);
    }

    private static void VerifyServicesCalls(
        Mock<IRecaptchaTokenExtractionService> tokenExtractionService,
        Mock<IRecaptchaVerificationService> verificationService,
        Mock<IRecaptchaVerificationResultValidationService> validationService,
        bool validationExecuted,
        bool useCancellationToken,
        string? action,
        float? score)
    {
        tokenExtractionService.Verify(x => x.GetToken(It.IsAny<ActionExecutingContext>()), Times.Once);

        verificationService.Verify(
            x => x.VerifyAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);

        verificationService.Verify(
            x => x.VerifyAsync(RecaptchaAttributeFixture.Token, ActionExecutingContextFixture.IPAddress.ToString(),
                useCancellationToken ? It.Is<CancellationToken>(ct => ct != default) : default),
            Times.Once);

        var validationTimes = validationExecuted ? Times.Once() : Times.Never();

        validationService.Verify(x => x.Validate(It.IsAny<VerifyResponse>(), It.IsAny<string?>(), It.IsAny<float?>()), validationTimes);

        validationService.Verify(x => x.Validate(It.IsAny<VerifyResponse>(), action, score), validationTimes);
    }
}
