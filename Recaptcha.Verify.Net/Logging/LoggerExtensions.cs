using Microsoft.Extensions.Logging;
using Recaptcha.Verify.Net.Client.Models.Request;
using Recaptcha.Verify.Net.Client.Models.Response;
using Recaptcha.Verify.Net.Service.Models;

namespace Recaptcha.Verify.Net.Logging;

internal static class LoggerExtensions
{
    private static readonly Action<ILogger, VerifyRequest, Exception?> _sendingRequest = LoggerMessage.Define<VerifyRequest>(
        logLevel: LogLevel.Trace,
        eventId: CoreEventId.SendingRequest,
        formatString: "Sending verify request. Request data: {Data}.");

    private static readonly Action<ILogger, string, string?, bool, float?, VerifyResponse, Exception?> _requestCompleted = LoggerMessage.Define<string, string?, bool, float?, VerifyResponse>(
        logLevel: LogLevel.Information,
        eventId: CoreEventId.RequestCompleted,
        formatString: @"Request for {Type} captcha for action {Action} completed with result {Success} and score {Score}.
Response data: {Data}.");

    private static readonly Action<ILogger, string, string?, float?, CheckResult, Exception?> _responseChecked = LoggerMessage.Define<string, string?, float?, CheckResult>(
        logLevel: LogLevel.Information,
        eventId: CoreEventId.ResponseChecked,
        formatString: @"Verify response checked for {Type} captcha request.
Request action {RequestAction}. Score threshold {ScoreThreshold}.
Result: {Result}.");

    public static void SendingRequest(this ILogger logger, VerifyRequest request) => _sendingRequest(logger, request, null);

    public static void RequestCompleted(this ILogger logger, VerifyResponse response) => _requestCompleted(
        logger,
        GetVersionString(response.IsV3),
        response.Action,
        response.Success,
        response.Score,
        response,
        null);

    public static void ResponseChecked(this ILogger logger, string? action, float? scoreThreshold, CheckResult checkResult) => _responseChecked(
        logger,
        GetVersionString(checkResult.Response.IsV3),
        action,
        scoreThreshold,
        checkResult,
        null);

    private static string GetVersionString(bool isV3) => isV3 ? "v3" : "v2";
}
