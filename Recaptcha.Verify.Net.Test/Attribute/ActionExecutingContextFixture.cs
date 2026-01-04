using HttpContextMoq;
using HttpContextMoq.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.TokenExtraction;
using Recaptcha.Verify.Net.TokenVerification;
using Recaptcha.Verify.Net.VerificationResultValidation;
using System.Net;

namespace Recaptcha.Verify.Net.Test.Attribute;

internal class ActionExecutingContextFixture
{
    public static readonly IPAddress IPAddress = new(int.MaxValue);

    public static IRecaptchaTokenExtractor CreateTokenExtractor(string? token)
    {
        var extractor = new Mock<IRecaptchaTokenExtractor>();

        extractor.Setup(x => x.GetToken(It.IsAny<ActionExecutingContext>())).Returns(token);

        return extractor.Object;
    }

    public static (ActionExecutingContext context, Mock<ActionExecutionDelegate> nextMock) CreateActionExecutingContext(
        RecaptchaOptions? options,
        IEnumerable<IRecaptchaTokenExtractor>? tokenExtractors,
        IRecaptchaVerificationService? verificationService,
        IRecaptchaVerificationResultValidationService? validationService)
    {
        var actionsArguments = new Dictionary<string, object?>();

        var httpContext = CreateHttpContext(options, tokenExtractors, verificationService, validationService);

        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor(),
        };

        var metadata = new List<IFilterMetadata>();

        var controller = new object();

        var context = new ActionExecutingContext(actionContext, metadata, actionsArguments, controller);

        var next = new Mock<ActionExecutionDelegate>();

        next.Setup(x => x.Invoke()).ReturnsAsync(new ActionExecutedContext(actionContext, metadata, controller));

        return (context, next);
    }

    private static HttpContextMock CreateHttpContext(
        RecaptchaOptions? options,
        IEnumerable<IRecaptchaTokenExtractor>? tokenExtractors,
        IRecaptchaVerificationService? verificationService,
        IRecaptchaVerificationResultValidationService? validationService)
    {
        var httpContext = new HttpContextMock();

        if (options is not null)
        {
            httpContext.SetupRequestService(Options.Create(options));
        }

        httpContext.SetupRequestService(tokenExtractors ?? []);

        if (verificationService is not null)
        {
            httpContext.SetupRequestService(verificationService);
        }

        if (validationService is not null)
        {
            httpContext.SetupRequestService(validationService);
        }

        httpContext.ConnectionMock.Mock.SetupGet(x => x.RemoteIpAddress).Returns(IPAddress);

        return httpContext;
    }
}
