using Microsoft.AspNetCore.Mvc;
using Moq;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.Exceptions;
using Recaptcha.Verify.Net.TokenExtraction;
using Xunit;

namespace Recaptcha.Verify.Net.Test.Tests;

public class AttributeTest : BaseRecaptchaAttributeTest
{
    [Fact]
    public void GetResponseToken()
    {
        var tokenParamName = RecaptchaServiceFixture.TokenParamName;
        var token = RecaptchaServiceFixture.Token;

        (var actionExecutingContext, _) = RecaptchaServiceFixture.CreateActionExecutingContext();

        var resultToken = new FormTokenExtractor(tokenParamName).GetToken(actionExecutingContext);
        Assert.Equal(token, resultToken);

        resultToken = new QueryTokenExtractor(tokenParamName).GetToken(actionExecutingContext);
        Assert.Equal(token, resultToken);

        resultToken = new HeaderTokenExtractor(tokenParamName).GetToken(actionExecutingContext);
        Assert.Equal(token, resultToken);

        resultToken = new ExecutingContextTokenExtractor((context) => token).GetToken(actionExecutingContext);
        Assert.Equal(token, resultToken);

        resultToken = new ActionArgumentsTokenExtractor((dict) => dict[tokenParamName]?.ToString()).GetToken(actionExecutingContext);
        Assert.Equal(token, resultToken);

        resultToken = new ActionArgumentsTokenExtractor(tokenParamName).GetToken(actionExecutingContext);
        Assert.Equal(token, resultToken);
    }

    [Theory]
    [MemberData(nameof(AttributeDataWithoutScore))]
    public async Task VerifyAndCheckAsyncExecuted_WithoutNotSpecifiedScoreInAttribute(RecaptchaAttribute attribute)
    {
        // Arrange
        (var recaptchaService, var verifyWithScoreFromAttribute, var verifyWithoutScoreFromAttribute) = RecaptchaServiceFixture.CreateService();

        (var actionExecutingContext, var next) = RecaptchaServiceFixture.CreateActionExecutingContext(recaptchaService.Object);

        // Act
        await attribute.OnActionExecutionAsync(actionExecutingContext, next);

        // Assert
        recaptchaService.Verify(verifyWithScoreFromAttribute, Times.Never);
        recaptchaService.Verify(verifyWithoutScoreFromAttribute, Times.Once);
    }

    [Theory]
    [MemberData(nameof(AttributeDataWithScore))]
    public async Task VerifyAndCheckAsyncExecuted_WithSpecifiedScoreInAttribute(RecaptchaAttribute attribute)
    {
        // Arrange
        (var recaptchaService, var verifyWithScoreFromAttribute, var verifyWithoutScoreFromAttribute) = RecaptchaServiceFixture.CreateService();

        (var actionExecutingContext, var next) = RecaptchaServiceFixture.CreateActionExecutingContext(recaptchaService.Object);

        // Act
        await attribute.OnActionExecutionAsync(actionExecutingContext, next);

        // Assert
        recaptchaService.Verify(verifyWithScoreFromAttribute, Times.Once);
        recaptchaService.Verify(verifyWithoutScoreFromAttribute, Times.Never);
    }

    [Theory]
    [MemberData(nameof(AttributeData))]
    public async Task VerifyAndCheckAsyncExecuted_RecaptchaServiceExceptionThrown(RecaptchaAttribute attribute)
    {
        // Arrange
        (var onRecaptchaServiceExceptionMock, var onRecaptchaServiceException) = RecaptchaServiceFixture.CreateOnRecaptchaServiceException();
        (var onExceptionMock, var onException) = RecaptchaServiceFixture.CreateOnException();
        (var onVerificationFailedMock, var onVerificationFailed) = RecaptchaServiceFixture.CreateOnVerificationFailed();
        (var onReturnBadRequestMock, var onReturnBadRequest) = RecaptchaServiceFixture.CreateOnReturnBadRequest();

        var options = new RecaptchaOptions
        {
            AttributeOptions = new RecaptchaAttributeOptions
            {
                OnRecaptchaServiceException = onRecaptchaServiceExceptionMock.Object,
                OnException = onExceptionMock.Object,
                OnVerificationFailed = onVerificationFailedMock.Object,
                OnReturnBadRequest = onReturnBadRequestMock.Object,
            }
        };

        (var recaptchaService, var verifyWithScoreFromAttribute, var verifyWithoutScoreFromAttribute) = RecaptchaServiceFixture.CreateService(typeof(RecaptchaServiceException));

        (var actionExecutingContext, var next) = RecaptchaServiceFixture.CreateActionExecutingContext(recaptchaService.Object, options);

        // Act
        await attribute.OnActionExecutionAsync(actionExecutingContext, next);

        // Assert
        Assert.IsType<BadRequestObjectResult>(actionExecutingContext.Result);
        Assert.NotNull(actionExecutingContext.Result);
        Assert.Equal((actionExecutingContext.Result as BadRequestObjectResult)!.Value, options.VerificationFailedMessage);

        onRecaptchaServiceExceptionMock.Verify(onRecaptchaServiceException, Times.Once);
        onExceptionMock.Verify(onException, Times.Never);
        onVerificationFailedMock.Verify(onVerificationFailed, Times.Never);
        onReturnBadRequestMock.Verify(onReturnBadRequest, Times.Once);
    }

    [Theory]
    [MemberData(nameof(AttributeData))]
    public async Task VerifyAndCheckAsyncExecuted_NonServiceExceptionThrown(RecaptchaAttribute attribute)
    {
        // Arrange
        (var onRecaptchaServiceExceptionMock, var onRecaptchaServiceException) = RecaptchaServiceFixture.CreateOnRecaptchaServiceException();
        (var onExceptionMock, var onException) = RecaptchaServiceFixture.CreateOnException();
        (var onVerificationFailedMock, var onVerificationFailed) = RecaptchaServiceFixture.CreateOnVerificationFailed();
        (var onReturnBadRequestMock, var onReturnBadRequest) = RecaptchaServiceFixture.CreateOnReturnBadRequest();

        var options = new RecaptchaOptions
        {
            AttributeOptions = new RecaptchaAttributeOptions
            {
                OnRecaptchaServiceException = onRecaptchaServiceExceptionMock.Object,
                OnException = onExceptionMock.Object,
                OnVerificationFailed = onVerificationFailedMock.Object,
                OnReturnBadRequest = onReturnBadRequestMock.Object,
            }
        };

        (var recaptchaService, var verifyWithScoreFromAttribute, var verifyWithoutScoreFromAttribute) = RecaptchaServiceFixture.CreateService(typeof(ArgumentNullException));

        (var actionExecutingContext, var next) = RecaptchaServiceFixture.CreateActionExecutingContext(recaptchaService.Object, options);

        // Act
        await attribute.OnActionExecutionAsync(actionExecutingContext, next);

        // Assert
        Assert.IsType<BadRequestObjectResult>(actionExecutingContext.Result);
        Assert.NotNull(actionExecutingContext.Result);
        Assert.Equal((actionExecutingContext.Result as BadRequestObjectResult)!.Value, options.VerificationFailedMessage);

        onRecaptchaServiceExceptionMock.Verify(onRecaptchaServiceException, Times.Never);
        onExceptionMock.Verify(onException, Times.Once);
        onVerificationFailedMock.Verify(onVerificationFailed, Times.Never);
        onReturnBadRequestMock.Verify(onReturnBadRequest, Times.Once);
    }

    [Theory]
    [MemberData(nameof(AttributeData))]
    public async Task VerifyAndCheckAsyncExecuted_BadResultReturned(RecaptchaAttribute attribute)
    {
        // Arrange
        (var onRecaptchaServiceExceptionMock, var onRecaptchaServiceException) = RecaptchaServiceFixture.CreateOnRecaptchaServiceException();
        (var onExceptionMock, var onException) = RecaptchaServiceFixture.CreateOnException();
        (var onVerificationFailedMock, var onVerificationFailed) = RecaptchaServiceFixture.CreateOnVerificationFailed();
        (var onReturnBadRequestMock, var onReturnBadRequest) = RecaptchaServiceFixture.CreateOnReturnBadRequest();

        var options = new RecaptchaOptions
        {
            AttributeOptions = new RecaptchaAttributeOptions
            {
                OnRecaptchaServiceException = onRecaptchaServiceExceptionMock.Object,
                OnException = onExceptionMock.Object,
                OnVerificationFailed = onVerificationFailedMock.Object,
                OnReturnBadRequest = onReturnBadRequestMock.Object,
            }
        };

        (var recaptchaService, var verifyWithScoreFromAttribute, var verifyWithoutScoreFromAttribute) = RecaptchaServiceFixture.CreateService();

        (var actionExecutingContext, var next) = RecaptchaServiceFixture.CreateActionExecutingContext(recaptchaService.Object, options);

        // Act
        await attribute.OnActionExecutionAsync(actionExecutingContext, next);

        // Assert
        Assert.IsType<BadRequestObjectResult>(actionExecutingContext.Result);
        Assert.NotNull(actionExecutingContext.Result);
        Assert.Equal((actionExecutingContext.Result as BadRequestObjectResult)!.Value, options.VerificationFailedMessage);

        onRecaptchaServiceExceptionMock.Verify(onRecaptchaServiceException, Times.Never);
        onExceptionMock.Verify(onException, Times.Never);
        onVerificationFailedMock.Verify(onVerificationFailed, Times.Once);
        onReturnBadRequestMock.Verify(onReturnBadRequest, Times.Once);
    }

    [Theory]
    [MemberData(nameof(AttributeData))]
    public async Task VerifyAndCheckAsyncExecuted_Success(RecaptchaAttribute attribute)
    {
        // Arrange
        (var onRecaptchaServiceExceptionMock, var onRecaptchaServiceException) = RecaptchaServiceFixture.CreateOnRecaptchaServiceException();
        (var onExceptionMock, var onException) = RecaptchaServiceFixture.CreateOnException();
        (var onVerificationFailedMock, var onVerificationFailed) = RecaptchaServiceFixture.CreateOnVerificationFailed();
        (var onReturnBadRequestMock, var onReturnBadRequest) = RecaptchaServiceFixture.CreateOnReturnBadRequest();

        var options = new RecaptchaOptions
        {
            AttributeOptions = new RecaptchaAttributeOptions
            {
                OnRecaptchaServiceException = onRecaptchaServiceExceptionMock.Object,
                OnException = onExceptionMock.Object,
                OnVerificationFailed = onVerificationFailedMock.Object,
                OnReturnBadRequest = onReturnBadRequestMock.Object,
            }
        };

        (var recaptchaService, var verifyWithScoreFromAttribute, var verifyWithoutScoreFromAttribute) = RecaptchaServiceFixture.CreateService(true);

        (var actionExecutingContext, var next) = RecaptchaServiceFixture.CreateActionExecutingContext(recaptchaService.Object, options);

        // Act
        await attribute.OnActionExecutionAsync(actionExecutingContext, next);

        // Assert
        Assert.Null(actionExecutingContext.Result);

        onRecaptchaServiceExceptionMock.Verify(onRecaptchaServiceException, Times.Never);
        onExceptionMock.Verify(onException, Times.Never);
        onVerificationFailedMock.Verify(onVerificationFailed, Times.Never);
        onReturnBadRequestMock.Verify(onReturnBadRequest, Times.Never);
    }
}