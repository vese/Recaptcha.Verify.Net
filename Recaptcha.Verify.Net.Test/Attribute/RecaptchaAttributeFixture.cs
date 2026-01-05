using Moq;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.TokenVerification;
using Recaptcha.Verify.Net.TokenVerification.Client.Models.Response;
using Recaptcha.Verify.Net.VerificationResultValidation;
using Recaptcha.Verify.Net.VerificationResultValidation.Models;
using RecaptchaServices = (
    Recaptcha.Verify.Net.TokenExtraction.IRecaptchaTokenExtractor[] tokenExtractors,
    Moq.Mock<Recaptcha.Verify.Net.TokenVerification.IRecaptchaVerificationService> verificationService,
    Moq.Mock<Recaptcha.Verify.Net.VerificationResultValidation.IRecaptchaVerificationResultValidationService> validationService);

namespace Recaptcha.Verify.Net.Test.Attribute;

internal static class RecaptchaAttributeFixture
{
    public const string Token = "Token";
    public const string Action = "Action";
    public const float Score = 0.5f;

    public static RecaptchaOptions GetRecaptchaOptions(bool useCancellationToken = false) => new()
    {
        AttributeOptions = new()
        {
            UseCancellationToken = useCancellationToken
        }
    };

    public static RecaptchaServices CreateServices(string?[] tokens,
        VerifyResponse? verificationResult = null, ValidationResult? validationResult = null,
        Exception? verificationException = null, Exception? validationException = null)
    {
        var tokenExtractors = tokens.Select(ActionExecutingContextFixture.CreateTokenExtractor).ToArray();

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

        return (tokenExtractors, verificationService, validationService);
    }
}
