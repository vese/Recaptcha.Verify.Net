using HttpContextMoq;
using HttpContextMoq.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace Recaptcha.Verify.Net.Test.TokenExtraction;

internal class ActionExecutingContextFixture
{
    public const string ActionArgumentsTokenName = "ActionArgumentsToken";
    public const string ActionArgumentsTokenValue = "ActionArgumentsTokenValue";
    public const string FormTokenName = "FormToken";
    public const string FormTokenValue = "FormTokenValue";
    public const string HeaderTokenName = "HeaderToken";
    public const string HeaderTokenValue = "HeaderTokenValue";
    public const string QueryTokenName = "QueryToken";
    public const string QueryTokenValue = "QueryTokenValue";

    public static ActionExecutingContext CreateActionExecutingContext()
    {
        var actionsArguments = new Dictionary<string, object?>() { { ActionArgumentsTokenName, ActionArgumentsTokenValue } };

        var httpContext = CreateHttpContext();

        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor(),
        };

        var metadata = new List<IFilterMetadata>();

        var controller = new object();

        return new ActionExecutingContext(actionContext, metadata, actionsArguments, controller);
    }

    private static HttpContextMock CreateHttpContext()
    {
        var httpContext = new HttpContextMock();

        httpContext.SetupRequestHeaders(new HeaderDictionary(new Dictionary<string, StringValues>
        {
            { HeaderTokenName, HeaderTokenValue}
        }));

        httpContext.RequestMock.Mock.SetupGet(x => x.HasFormContentType).Returns(true);
        httpContext.RequestMock.Form = new FormCollection(new Dictionary<string, StringValues>
        {
            { FormTokenName, FormTokenValue}
        });

        httpContext.RequestMock.Query = new QueryCollection(new Dictionary<string, StringValues>
        {
            { QueryTokenName, QueryTokenValue}
        });

        return httpContext;
    }
}
