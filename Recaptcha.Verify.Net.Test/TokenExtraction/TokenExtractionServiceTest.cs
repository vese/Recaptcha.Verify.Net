using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using Recaptcha.Verify.Net.Exceptions.Configuration;
using Recaptcha.Verify.Net.Exceptions.Processing;
using Recaptcha.Verify.Net.TokenExtraction;
using Xunit;

namespace Recaptcha.Verify.Net.Test.TokenExtraction;

public class TokenExtractionServiceTest
{
    private const string Token = "TestToken";
    private const string OtherToken = "OtherToken";

    [Fact]
    public void GetToken_NoExtractors_ThrowsTokenExtractorNotFound()
    {
        var service = new RecaptchaTokenExtractionService([]);
        var context = ActionExecutingContextFixture.CreateActionExecutingContext();

        Assert.Throws<TokenExtractorNotFound>(() => service.GetToken(context));
    }

    [Fact]
    public void GetToken_AllExtractorsReturnEmpty_ThrowsEmptyCaptchaAnswer()
    {
        var extractors = new[]
        {
            CreateExtractor(null).Object,
            CreateExtractor(string.Empty).Object,
            CreateExtractor("   ").Object,
        };
        var service = new RecaptchaTokenExtractionService(extractors);
        var context = ActionExecutingContextFixture.CreateActionExecutingContext();

        Assert.Throws<EmptyCaptchaAnswerException>(() => service.GetToken(context));
    }

    [Fact]
    public void GetToken_SingleExtractorReturnsToken_ReturnsToken()
    {
        var service = new RecaptchaTokenExtractionService([CreateExtractor(Token).Object]);
        var context = ActionExecutingContextFixture.CreateActionExecutingContext();

        var result = service.GetToken(context);

        Assert.Equal(Token, result);
    }

    [Fact]
    public void GetToken_FirstEmptySecondReturnsToken_ReturnsToken()
    {
        var service = new RecaptchaTokenExtractionService([CreateExtractor(null).Object, CreateExtractor(Token).Object]);
        var context = ActionExecutingContextFixture.CreateActionExecutingContext();

        var result = service.GetToken(context);

        Assert.Equal(Token, result);
    }

    [Fact]
    public void GetToken_FirstWins_SecondExtractorNotCalled()
    {
        var firstExtractor = CreateExtractor(Token);
        var secondExtractor = CreateExtractor(OtherToken);

        var service = new RecaptchaTokenExtractionService([firstExtractor.Object, secondExtractor.Object]);
        var context = ActionExecutingContextFixture.CreateActionExecutingContext();

        var result = service.GetToken(context);

        Assert.Equal(Token, result);
        firstExtractor.Verify(x => x.GetToken(context), Times.Once);
        secondExtractor.Verify(x => x.GetToken(It.IsAny<ActionExecutingContext>()), Times.Never);
    }

    private static Mock<IRecaptchaTokenExtractor> CreateExtractor(string? token)
    {
        var extractor = new Mock<IRecaptchaTokenExtractor>();
        extractor.Setup(x => x.GetToken(It.IsAny<ActionExecutingContext>())).Returns(token);
        return extractor;
    }
}
