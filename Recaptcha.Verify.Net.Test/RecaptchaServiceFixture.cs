using HttpContextMoq;
using HttpContextMoq.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.Exceptions;
using Recaptcha.Verify.Net.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Recaptcha.Verify.Net.Test;

public record ResponseTokenFixture(string Token, string? Action, string? Hostname, string? ApkPackageName, float? Score, List<string>? Errors)
{
    public List<VerifyError> ErrorsItems => VerifyErrorHelper.GetVerifyErrors(Errors);
}

public static class RecaptchaServiceFixture
{
    /// <summary>
    /// Test secret key for reCAPTCHA v2.
    /// https://developers.google.com/recaptcha/docs/faq#id-like-to-run-automated-tests-with-recaptcha.-what-should-i-do
    /// </summary>
    public const string ValidSecretKey = "6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe";
    public const string InvalidSecretKey = "<invalid secret key>";
    public const string Token = "token";
    public const string Action = "action";
    public const float Score = 0.5f;
    public const float ScoreUnsuccessful = 2.0f;
    public const string Hostname = "hostname";
    public const string ApkPackageName = "action";
    public const string TokenParamName = "token-param";
    public static readonly IPAddress IPAddress = IPAddress.Any;

    public static Dictionary<string, float> ScoreMap => new()
    {
        { ValidTokenFixture10.Action!, 0.9f },
        { ValidTokenFixture05.Action!, 0.5f },
        { ValidTokenFixture02.Action!, 0.3f },
    };

    public static List<string> SecretKeys => new() { InvalidSecretKey, ValidSecretKey };

    public static readonly List<string> ErrorsKeys = VerifyErrorHelper.VerifyErrorsDictionary.Keys.ToList();
    public static readonly string UnknownErrorKey = "unknown-error-key";

    public static readonly ResponseTokenFixture InvalidTokenFixtureUnknownErrorKey =
        new("token1", null, null, null, null, new() { UnknownErrorKey });
    public static readonly ResponseTokenFixture InvalidTokenFixtureOneKey =
        new("token2", null, null, null, null, new() { ErrorsKeys.First() });
    public static readonly ResponseTokenFixture InvalidTokenFixtureAllKeys =
        new("token3", null, null, null, null, ErrorsKeys);

    public static readonly ResponseTokenFixture ValidTokenFixture10 =
        new("token4", "action4", Hostname, null, 1.0f, null);
    public static readonly ResponseTokenFixture ValidTokenFixture05 =
        new("token5", "action5", Hostname, null, 0.5f, null);
    public static readonly ResponseTokenFixture ValidTokenFixture02 =
        new("token6", "action6", null, ApkPackageName, 0.2f, null);

    public static List<ResponseTokenFixture> AllTokenFixtures => new()
    {
        InvalidTokenFixtureUnknownErrorKey,
        InvalidTokenFixtureOneKey,
        InvalidTokenFixtureAllKeys,
        ValidTokenFixture10,
        ValidTokenFixture05,
        ValidTokenFixture02,
    };

    public static List<ResponseTokenFixture> ValidTokenFixtures => new()
    {
        ValidTokenFixture10,
        ValidTokenFixture05,
        ValidTokenFixture02,
    };

    public static List<ResponseTokenFixture> InvalidTokenFixtures => new()
    {
        InvalidTokenFixtureOneKey,
        InvalidTokenFixtureAllKeys,
    };

    public static IRecaptchaService CreateServiceWithClient(IOptions<RecaptchaOptions> options, bool v3 = true)
    {
        var recaptchaClientMock = new Mock<IRecaptchaClient>();

        recaptchaClientMock
            .Setup(x => x.VerifyAsync(
                It.Is<VerifyRequest>(req => req.Secret == InvalidSecretKey),
                default))
            .Returns(Task.FromResult(new VerifyResponse() { Success = false, Score = null }));

        foreach (var tokenFixture in AllTokenFixtures)
        {
            recaptchaClientMock
                .Setup(x => x.VerifyAsync(
                    It.Is<VerifyRequest>(req => req.Secret == ValidSecretKey && req.Response == tokenFixture.Token),
                    default))
                .Returns(Task.FromResult(new VerifyResponse()
                {
                    Success = tokenFixture.Score != null,
                    Score = v3 ? tokenFixture.Score : null,
                    Action = v3 ? tokenFixture.Action : null,
                    ChallengeTs = DateTime.Now,
                    Hostname = tokenFixture.Hostname,
                    ApkPackageName = tokenFixture.ApkPackageName,
                    ErrorCodes = tokenFixture.Errors,
                }));
        }

        return new RecaptchaService(options, recaptchaClientMock.Object, NullLoggerFactory.Instance.CreateLogger<RecaptchaService>());
    }

    public static (
        Mock<Func<ActionExecutingContext, string, CheckResult, RecaptchaServiceException, IActionResult>> mock,
        Expression<Func<Func<ActionExecutingContext, string, CheckResult, RecaptchaServiceException, IActionResult>, IActionResult>> expression) CreateOnRecaptchaServiceException()
    {
        var mock = new Mock<Func<ActionExecutingContext, string, CheckResult, RecaptchaServiceException, IActionResult>>();
        Expression<Func<Func<ActionExecutingContext, string, CheckResult, RecaptchaServiceException, IActionResult>, IActionResult>> expression =
            x => x.Invoke(It.IsAny<ActionExecutingContext>(), It.IsAny<string>(), It.IsAny<CheckResult>(), It.IsAny<RecaptchaServiceException>());
        mock.Setup(expression).Returns<IActionResult>(null);
        return (mock, expression);
    }

    public static (
        Mock<Func<ActionExecutingContext, string, CheckResult, Exception, IActionResult>> mock,
        Expression<Func<Func<ActionExecutingContext, string, CheckResult, Exception, IActionResult>, IActionResult>> expression) CreateOnException()
    {
        var mock = new Mock<Func<ActionExecutingContext, string, CheckResult, Exception, IActionResult>>();
        Expression<Func<Func<ActionExecutingContext, string, CheckResult, Exception, IActionResult>, IActionResult>> expression =
            x => x.Invoke(It.IsAny<ActionExecutingContext>(), It.IsAny<string>(), It.IsAny<CheckResult>(), It.IsAny<Exception>());
        mock.Setup(expression).Returns<IActionResult>(null);
        return (mock, expression);
    }

    public static (
        Mock<Func<ActionExecutingContext, string, CheckResult, IActionResult>> mock,
        Expression<Func<Func<ActionExecutingContext, string, CheckResult, IActionResult>, IActionResult>> expression) CreateOnVerificationFailed()
    {
        var mock = new Mock<Func<ActionExecutingContext, string, CheckResult, IActionResult>>();
        Expression<Func<Func<ActionExecutingContext, string, CheckResult, IActionResult>, IActionResult>> expression =
            x => x.Invoke(It.IsAny<ActionExecutingContext>(), It.IsAny<string>(), It.IsAny<CheckResult>());
        mock.Setup(expression).Returns<IActionResult>(null);
        return (mock, expression);
    }

    public static (
        Mock<Func<ActionExecutingContext, string, CheckResult, RecaptchaServiceException, Exception, IActionResult>> mock,
        Expression<Func<Func<ActionExecutingContext, string, CheckResult, RecaptchaServiceException, Exception, IActionResult>, IActionResult>> expression) CreateOnReturnBadRequest()
    {
        var mock = new Mock<Func<ActionExecutingContext, string, CheckResult, RecaptchaServiceException, Exception, IActionResult>>();
        Expression<Func<Func<ActionExecutingContext, string, CheckResult, RecaptchaServiceException, Exception, IActionResult>, IActionResult>> expression =
            x => x.Invoke(It.IsAny<ActionExecutingContext>(), It.IsAny<string>(), It.IsAny<CheckResult>(), It.IsAny<RecaptchaServiceException>(), It.IsAny<Exception>());
        mock.Setup(expression).Returns<IActionResult>(null);
        return (mock, expression);
    }

    public static (
        Mock<IRecaptchaService> serviceMock,
        Expression<Func<IRecaptchaService, Task<CheckResult>>> verifyWithScoreFromAttribute,
        Expression<Func<IRecaptchaService, Task<CheckResult>>> verifyWithoutScoreFromAttribute) CreateService(bool checkResult = false) =>
        CreateService(checkResult, null);

    public static (
        Mock<IRecaptchaService> serviceMock,
        Expression<Func<IRecaptchaService, Task<CheckResult>>> verifyWithScoreFromAttribute,
        Expression<Func<IRecaptchaService, Task<CheckResult>>> verifyWithoutScoreFromAttribute) CreateService(Type exceptionType) =>
        CreateService(false, exceptionType);

    private static (
        Mock<IRecaptchaService> serviceMock,
        Expression<Func<IRecaptchaService, Task<CheckResult>>> verifyWithScoreFromAttribute,
        Expression<Func<IRecaptchaService, Task<CheckResult>>> verifyWithoutScoreFromAttribute) CreateService(bool checkResult, Type? exceptionType)
    {
        var setups = new List<Moq.Language.Flow.ISetup<IRecaptchaService, Task<CheckResult>>>();

        var recaptchaService = new Mock<IRecaptchaService>();

        Expression<Func<IRecaptchaService, Task<CheckResult>>> verifyWithScoreFromAttribute =
            x => x.VerifyAndCheckAsync(It.IsAny<VerifyRequest>(), It.IsAny<string>(), It.IsAny<float>(), It.IsAny<CancellationToken>());
        setups.Add(recaptchaService.Setup(verifyWithScoreFromAttribute));

        Expression<Func<IRecaptchaService, Task<CheckResult>>> verifyWithoutScoreFromAttribute =
            x => x.VerifyAndCheckAsync(It.IsAny<VerifyRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>());
        setups.Add(recaptchaService.Setup(verifyWithoutScoreFromAttribute));

        if (exceptionType != null)
        {
            var exception = Activator.CreateInstance(exceptionType) as Exception;
            setups.ForEach(setup => setup.ThrowsAsync(exception));
        }
        else
        {
            var result = new CheckResult
            {
                Response = new()
                {
                    Success = checkResult,
                },
                ActionMatches = checkResult,
                ScoreSatisfies = checkResult,
            };
            setups.ForEach(setup => setup.ReturnsAsync(result));
        }

        return (recaptchaService, verifyWithScoreFromAttribute, verifyWithoutScoreFromAttribute);
    }

    public static (ActionExecutingContext context, ActionExecutionDelegate next) CreateActionExecutingContext(IRecaptchaService? recaptchaService = null, RecaptchaOptions? options = null)
    {
        var actionsArguments = new Dictionary<string, object?>() { { TokenParamName, Token } };

        var httpContext = CreateHttpContext(recaptchaService, options, TokenParamName, Token);

        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor(),
        };

        var metadata = new List<IFilterMetadata>();

        var controller = new object();

        var context = new ActionExecutingContext(actionContext, metadata, actionsArguments, controller);

        ActionExecutionDelegate next = () =>
        {
            var executedContext = new ActionExecutedContext(actionContext, metadata, controller);
            return Task.FromResult(executedContext);
        };

        return (context, next);
    }

    private static HttpContextMock CreateHttpContext(IRecaptchaService? recaptchaService, RecaptchaOptions? options, string paramName, string tokenValue)
    {
        var httpContext = new HttpContextMock();

        options ??= new RecaptchaOptions();
        httpContext.SetupRequestService(Options.Create(options));

        if (recaptchaService != null)
        {
            httpContext.SetupRequestService(recaptchaService);
        }

        StringValues token = tokenValue;
        httpContext.RequestMock.HeadersMock.Mock.Setup(m => m.TryGetValue(It.Is<string>(a => a == paramName), out token));
        httpContext.RequestMock.QueryMock.Mock.Setup(m => m.TryGetValue(It.Is<string>(a => a == paramName), out token));
        httpContext.RequestMock.FormMock.Mock.Setup(m => m.TryGetValue(It.Is<string>(a => a == paramName), out token));
        httpContext.RequestMock.Mock.Setup(m => m.HasFormContentType).Returns(true);
        httpContext.ConnectionMock.Mock.Setup(m => m.RemoteIpAddress).Returns(IPAddress);

        return httpContext;
    }
}
