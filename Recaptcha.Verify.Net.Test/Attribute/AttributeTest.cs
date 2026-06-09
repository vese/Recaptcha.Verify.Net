using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using Recaptcha.Verify.Net.Attribute;
using Recaptcha.Verify.Net.Configuration;
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
    public async Task Execute_NoTokenExtractionService_ReturnsBadRequest()
    {
        var options = RecaptchaAttributeFixture.GetRecaptchaOptions();

        (var context, var next) = ActionExecutingContextFixture.CreateActionExecutingContext(options, null, null, null);

        var attribute = new RecaptchaAttribute();

        await attribute.OnActionExecutionAsync(context, next.Object);

        next.Verify(x => x.Invoke(), Times.Never);

        Assert.NotNull(context.Result);
        Assert.True(context.Result is BadRequestObjectResult);
        Assert.Equal(StatusCodes.Status400BadRequest, (context.Result as BadRequestObjectResult)!.StatusCode);
    }

    [Fact]
    public async Task Execute_TokenExtractionServiceThrowsTokenExtractorNotFound_ReturnsBadRequest()
    {
        (var tokenExtractionService, var verificationService, var validationService) = RecaptchaAttributeFixture.CreateServices(
            null, extractionException: new TokenExtractorNotFound());

        var options = RecaptchaAttributeFixture.GetRecaptchaOptions();

        (var context, var next) = ActionExecutingContextFixture.CreateActionExecutingContext(
            options, tokenExtractionService.Object, verificationService.Object, validationService.Object);

        var attribute = new RecaptchaAttribute();

        await attribute.OnActionExecutionAsync(context, next.Object);

        next.Verify(x => x.Invoke(), Times.Never);

        Assert.NotNull(context.Result);
        Assert.True(context.Result is BadRequestObjectResult);
        Assert.Equal(StatusCodes.Status400BadRequest, (context.Result as BadRequestObjectResult)!.StatusCode);
    }

    [Fact]
    public async Task Execute_TokenExtractionServiceThrowsEmptyCaptchaAnswer_ReturnsBadRequest()
    {
        (var tokenExtractionService, var verificationService, var validationService) = RecaptchaAttributeFixture.CreateServices(
            null, extractionException: new EmptyCaptchaAnswerException());

        var options = RecaptchaAttributeFixture.GetRecaptchaOptions();

        (var context, var next) = ActionExecutingContextFixture.CreateActionExecutingContext(
            options, tokenExtractionService.Object, verificationService.Object, validationService.Object);

        var attribute = new RecaptchaAttribute();

        await attribute.OnActionExecutionAsync(context, next.Object);

        next.Verify(x => x.Invoke(), Times.Never);

        Assert.NotNull(context.Result);
        Assert.True(context.Result is BadRequestObjectResult);
        Assert.Equal(StatusCodes.Status400BadRequest, (context.Result as BadRequestObjectResult)!.StatusCode);
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
    public async Task Execute_VerificationThrows_ReturnsBadRequest(bool useCancellationToken, string? action, float? score)
    {
        (var tokenExtractionService, var verificationService, var validationService) = RecaptchaAttributeFixture.CreateServices(
            RecaptchaAttributeFixture.Token,
            null,
            new ValidationResult
            {
                ResponseSuccessful = false,
                IsV3 = true
            },
            verificationException: new Exception());

        var options = RecaptchaAttributeFixture.GetRecaptchaOptions(useCancellationToken);

        (var context, var next) = ActionExecutingContextFixture.CreateActionExecutingContext(
            options, tokenExtractionService.Object, verificationService.Object, validationService.Object);

        var attribute = score.HasValue ? new RecaptchaAttribute(action!, score.Value) : new RecaptchaAttribute(action!);

        await attribute.OnActionExecutionAsync(context, next.Object);

        VerifyServicesCalls(tokenExtractionService, verificationService, validationService, false, useCancellationToken, action, score);

        next.Verify(x => x.Invoke(), Times.Never);

        Assert.NotNull(context.Result);
        Assert.True(context.Result is BadRequestObjectResult);
        Assert.Equal(StatusCodes.Status400BadRequest, (context.Result as BadRequestObjectResult)!.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetExecuteParameters))]
    public async Task Execute_ValidationThrows_ReturnsBadRequest(bool useCancellationToken, string? action, float? score)
    {
        (var tokenExtractionService, var verificationService, var validationService) = RecaptchaAttributeFixture.CreateServices(
            RecaptchaAttributeFixture.Token,
            new VerifyResponse(),
            validationException: new Exception());

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
