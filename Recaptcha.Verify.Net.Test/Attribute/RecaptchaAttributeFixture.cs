using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.TokenExtraction;
using Recaptcha.Verify.Net.TokenVerification;
using Recaptcha.Verify.Net.TokenVerification.Client.Models.Response;
using Recaptcha.Verify.Net.VerificationResultValidation;
using Recaptcha.Verify.Net.VerificationResultValidation.Models;
using RecaptchaServices = (
    Moq.Mock<Recaptcha.Verify.Net.TokenExtraction.IRecaptchaTokenExtractionService> tokenExtractionService,
    Moq.Mock<Recaptcha.Verify.Net.TokenVerification.IRecaptchaVerificationService> verificationService,
    Moq.Mock<Recaptcha.Verify.Net.VerificationResultValidation.IRecaptchaVerificationResultValidationService> validationService);

namespace Recaptcha.Verify.Net.Test.Attribute;

internal static class RecaptchaAttributeFixture
{
    public const string Token = "Token";
    public const string Action = "Action";
    public const float Score = 0.5f;

    public static RecaptchaAttributeOptions GetRecaptchaOptions(bool useCancellationToken = false) => new()
    {
        UseCancellationToken = useCancellationToken
    };

    public static RecaptchaServices CreateServices(string? token,
        VerifyResponse? verificationResult = null, ValidationResult? validationResult = null,
        Exception? extractionException = null, Exception? verificationException = null, Exception? validationException = null)
    {
        var tokenExtractionService = new Mock<IRecaptchaTokenExtractionService>();

        if (extractionException is not null)
        {
            tokenExtractionService.Setup(x => x.GetToken(It.IsAny<ActionExecutingContext>())).Throws(extractionException);
        }
        else
        {
            tokenExtractionService.Setup(x => x.GetToken(It.IsAny<ActionExecutingContext>())).Returns(token!);
        }

        var verificationService = new Mock<IRecaptchaVerificationService>();

        if (verificationResult is not null)
        {
            verificationService
                .Setup(x => x.VerifyAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(verificationResult);
        }
        else if (verificationException is not null)
        {
            verificationService
                .Setup(x => x.VerifyAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(verificationException);
        }

        var validationService = new Mock<IRecaptchaVerificationResultValidationService>();

        if (validationResult is not null)
        {
            validationService
                .Setup(x => x.Validate(It.IsAny<VerifyResponse>(), It.IsAny<string?>(), It.IsAny<float?>()))
                .Returns(validationResult);
        }
        else if (validationException is not null)
        {
            validationService
                .Setup(x => x.Validate(It.IsAny<VerifyResponse>(), It.IsAny<string?>(), It.IsAny<float?>()))
                .Throws(validationException);
        }

        return (tokenExtractionService, verificationService, validationService);
    }
}
