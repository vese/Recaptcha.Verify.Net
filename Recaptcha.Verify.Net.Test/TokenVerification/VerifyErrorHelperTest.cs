using Recaptcha.Verify.Net.TokenVerification.Client.Models.Response;
using Xunit;

namespace Recaptcha.Verify.Net.Test.TokenVerification;

public class VerifyErrorHelperTest
{
    [Fact]
    public void Errors_MapsKnownCodesToExpectedVerifyErrors()
    {
        var response = new VerifyResponse
        {
            ErrorCodes = ["missing-input-secret", "timeout-or-duplicate"]
        };

        var errors = response.Errors;

        Assert.NotNull(errors);
        Assert.Equal(
            new[] { VerifyError.MissingInputSecret, VerifyError.TimeoutOrDuplicate },
            errors!.ToArray());
    }

    [Fact]
    public void Errors_MapsUnknownCodeToUnknownWithoutThrowing()
    {
        var response = new VerifyResponse
        {
            ErrorCodes = ["brand-new-code"]
        };

        var ex = Record.Exception(() => response.Errors);

        Assert.Null(ex);
        Assert.NotNull(response.Errors);
        Assert.Equal([VerifyError.Unknown], response.Errors!.ToArray());
    }
}
