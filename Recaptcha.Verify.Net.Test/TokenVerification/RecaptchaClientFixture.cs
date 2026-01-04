using Moq;
using Recaptcha.Verify.Net.TokenVerification.Client;
using Recaptcha.Verify.Net.TokenVerification.Client.Models.Request;
using Recaptcha.Verify.Net.TokenVerification.Client.Models.Response;

namespace Recaptcha.Verify.Net.Test.TokenVerification;

internal static class RecaptchaClientFixture
{
    public static IRecaptchaClient Create(bool throwsException = false)
    {
        var recaptchaClientMock = new Mock<IRecaptchaClient>();

        if (throwsException)
        {
            recaptchaClientMock
                .Setup(x => x.VerifyAsync(
                    It.IsAny<VerifyRequest>(),
                    default))
                .ThrowsAsync(new HttpRequestException());
        }
        else
        {
            recaptchaClientMock
                .Setup(x => x.VerifyAsync(
                    It.Is<VerifyRequest>(req => req.Secret == TokenVerificationFixture.InvalidSecretKey),
                    default))
                .Returns(Task.FromResult(new VerifyResponse() { Success = false, Score = null }));

            recaptchaClientMock
                .Setup(x => x.VerifyAsync(
                    It.Is<VerifyRequest>(req => req.Secret == TokenVerificationFixture.ValidSecretKey && req.Response == TokenVerificationFixture.InvalidResponseToken),
                    default))
                .Returns(Task.FromResult(new VerifyResponse() { Success = false, Score = null }));

            foreach (var tokenScore in TokenVerificationFixture.ValidResponseTokens)
            {
                recaptchaClientMock
                    .Setup(x => x.VerifyAsync(
                        It.Is<VerifyRequest>(req => req.Secret == TokenVerificationFixture.ValidSecretKey && req.Response == tokenScore.Key),
                        default))
                    .Returns(Task.FromResult(new VerifyResponse() { Success = true, Score = tokenScore.Value }));
            }
        }

        return recaptchaClientMock.Object;
    }
}
