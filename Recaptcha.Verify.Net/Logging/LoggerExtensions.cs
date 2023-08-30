using Microsoft.Extensions.Logging;
using Recaptcha.Verify.Net.Exceptions;
using System;

namespace Recaptcha.Verify.Net.Logging
{
    internal static class LoggerExtensions
    {
        private const string _configurationExceptionMessage = "Recaptcha service configuration exception";
        private const string _exceptionMessage = "Recaptcha service exception";

        private static readonly Action<ILogger, SecretKeyNotSpecifiedException> _missingSecretKeyError = LoggerMessage.Define(
            logLevel: LogLevel.Error,
            eventId: CoreEventId.MissingSecretKeyError,
            formatString: _configurationExceptionMessage);

        private static readonly Action<ILogger, EmptyActionException> _missingActionError = LoggerMessage.Define(
            logLevel: LogLevel.Error,
            eventId: CoreEventId.MissingActionError,
            formatString: _configurationExceptionMessage);

        private static readonly Action<ILogger, MinScoreNotSpecifiedException> _missingMinScoreError = LoggerMessage.Define(
            logLevel: LogLevel.Error,
            eventId: CoreEventId.MissingMinScoreError,
            formatString: _configurationExceptionMessage);

        private static readonly Action<ILogger, VerifyRequest, Exception> _sendingRequest = LoggerMessage.Define<VerifyRequest>(
            logLevel: LogLevel.Trace,
            eventId: CoreEventId.SendingRequest,
            formatString: "Sending verify request. Request data: {Data}.");

        private static readonly Action<ILogger, string, string, bool, float?, VerifyResponse, Exception> _requestCompleted = LoggerMessage.Define<string, string, bool, float?, VerifyResponse>(
            logLevel: LogLevel.Information,
            eventId: CoreEventId.RequestCompleted,
            formatString: @"Request for {Type} captcha for action {Action} completed with result {Success} and score {Score}.
Response data: {Data}.");

        private static readonly Action<ILogger, EmptyCaptchaAnswerException> _missingCaptchaAnswerError = LoggerMessage.Define(
            logLevel: LogLevel.Error,
            eventId: CoreEventId.MissingCaptchaAnswerError,
            formatString: _exceptionMessage);

        private static readonly Action<ILogger, HttpRequestException> _httpRequestError = LoggerMessage.Define(
            logLevel: LogLevel.Error,
            eventId: CoreEventId.HttpRequestError,
            formatString: _exceptionMessage);

        private static readonly Action<ILogger, string, string, float?, CheckResult, Exception> _responseChecked = LoggerMessage.Define<string, string, float?, CheckResult>(
            logLevel: LogLevel.Information,
            eventId: CoreEventId.ResponseChecked,
            formatString: @"Verify response checked for {Type} captcha request.
Request action {RequestAction}. Score threshold {ScoreThreshold}.
Result: {Result}.");

        public static void MissingSecretKeyError(this ILogger logger, SecretKeyNotSpecifiedException e) => _missingSecretKeyError(logger, e);
        public static void MissingActionError(this ILogger logger, EmptyActionException e) => _missingActionError(logger, e);
        public static void MissingMinScoreError(this ILogger logger, MinScoreNotSpecifiedException e) => _missingMinScoreError(logger, e);
        public static void SendingRequest(this ILogger logger, VerifyRequest request) => _sendingRequest(logger, request, null);
        public static void RequestCompleted(this ILogger logger, VerifyResponse response) => _requestCompleted(
            logger,
            GetVersionString(response.IsV3),
            response.Action,
            response.Success,
            response.Score,
            response,
            null);
        public static void MissingCaptchaAnswerError(this ILogger logger, EmptyCaptchaAnswerException e) => _missingCaptchaAnswerError(logger, e);
        public static void HttpRequestError(this ILogger logger, HttpRequestException e) => _httpRequestError(logger, e);
        public static void ResponseChecked(this ILogger logger, string action, float? scoreThreshold, CheckResult checkResult) => _responseChecked(
            logger,
            GetVersionString(checkResult.Response.IsV3),
            action,
            scoreThreshold,
            checkResult,
            null);

        private static string GetVersionString(bool isV3) => isV3 ? "v3" : "v2";
    }
}
