using Recaptcha.Verify.Net.TokenExtraction;
using Xunit;

namespace Recaptcha.Verify.Net.Test.TokenExtraction;

public class TokenExtractionTest
{
    [Fact]
    public void Extract_FromActionArguments_WithAction()
    {
        var context = ActionExecutingContextFixture.CreateActionExecutingContext();

        var extractor = new ActionArgumentsTokenExtractor((dict) => dict[ActionExecutingContextFixture.ActionArgumentsTokenName]?.ToString());
        var resultToken = extractor.GetToken(context);

        Assert.Equal(ActionExecutingContextFixture.ActionArgumentsTokenValue, resultToken);
    }

    [Fact]
    public void Extract_FromActionArguments_WithName()
    {
        var context = ActionExecutingContextFixture.CreateActionExecutingContext();

        var extractor = new ActionArgumentsTokenExtractor(ActionExecutingContextFixture.ActionArgumentsTokenName);
        var resultToken = extractor.GetToken(context);

        Assert.Equal(ActionExecutingContextFixture.ActionArgumentsTokenValue, resultToken);
    }

    [Fact]
    public void Extract_FromExecutingContext()
    {
        var context = ActionExecutingContextFixture.CreateActionExecutingContext();

        var extractor = new ExecutingContextTokenExtractor(
            (context) => context.ActionArguments[ActionExecutingContextFixture.ActionArgumentsTokenName]?.ToString());
        var resultToken = extractor.GetToken(context);

        Assert.Equal(ActionExecutingContextFixture.ActionArgumentsTokenValue, resultToken);
    }

    [Fact]
    public void Extract_FromForm()
    {
        var context = ActionExecutingContextFixture.CreateActionExecutingContext();

        var extractor = new FormTokenExtractor(ActionExecutingContextFixture.FormTokenName);
        var resultToken = extractor.GetToken(context);

        Assert.Equal(ActionExecutingContextFixture.FormTokenValue, resultToken);
    }

    [Fact]
    public void Extract_FromHeader()
    {
        var context = ActionExecutingContextFixture.CreateActionExecutingContext();

        var extractor = new HeaderTokenExtractor(ActionExecutingContextFixture.HeaderTokenName);
        var resultToken = extractor.GetToken(context);

        Assert.Equal(ActionExecutingContextFixture.HeaderTokenValue, resultToken);
    }

    [Fact]
    public void Extract_FromQuery()
    {
        var context = ActionExecutingContextFixture.CreateActionExecutingContext();

        var extractor = new QueryTokenExtractor(ActionExecutingContextFixture.QueryTokenName);
        var resultToken = extractor.GetToken(context);

        Assert.Equal(ActionExecutingContextFixture.QueryTokenValue, resultToken);
    }
}
