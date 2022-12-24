using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.Exceptions;
using System;
using System.Collections.Generic;

namespace Recaptcha.Verify.Net.Logging
{
    public class RecaptchaLoggerService : IRecaptchaLoggerService
    {
        private static readonly Dictionary<EventId, string> _eventsLogMessageTemplates = new Dictionary<EventId, string>()
        {
            { RecaptchaServiceEventId.ServiceException, "Service exception was thrown." },
            { RecaptchaServiceEventId.SendingRequest, "Sending verify request. Response token: {0}, remote IP: {1}." },
            { RecaptchaServiceEventId.RequestSucceded, "Verify request succeded. Response: {0}." },
            { RecaptchaServiceEventId.VerifyResponseChecked, "Verify response checked. Result: {0}." },
        };

        private readonly LogOptions _options;
        private readonly ILogger _logger;

        public RecaptchaLoggerService(IOptions<LogOptions> options, ILoggerFactory loggerFactory)
        {
            _options = options.Value;
            if (_options.EnableLogging)
            {
                _logger = loggerFactory.CreateLogger<RecaptchaService>();
            }
        }

        public void Log(EventId eventId, Exception e, params object[] args)
        {
            if (!_options.EnableLogging)
            {
                return;
            }

            if (!_options.LogLevels.TryGetValue(eventId, out var logLevel))
            {
                throw CreateException($"Log level for event {eventId} is missing", eventId);
            }
            if (!_eventsLogMessageTemplates.TryGetValue(eventId, out var message))
            {
                throw CreateException($"Message for event {eventId} is missing", eventId);
            }

            if (eventId == RecaptchaServiceEventId.ServiceException)
            {
                if (!_options.EnableExceptionLogging)
                {
                    return;
                }
                if (_options.LogServiceExceptionMessageHandler != null)
                {
                    _options.LogServiceExceptionMessageHandler.Invoke(logLevel, eventId, message, e);
                    return;
                }
            }
            else if (eventId == RecaptchaServiceEventId.SendingRequest)
            {
                if (_options.LogSendingRequestMessageHandler != null)
                {
                    _options.LogSendingRequestMessageHandler.Invoke(logLevel, eventId, message, args[0] as string, args[1] as string);
                    return;
                }
            }
            else if (eventId == RecaptchaServiceEventId.RequestSucceded)
            {
                if (_options.LogRequestSuccededMessageHandler != null)
                {
                    _options.LogRequestSuccededMessageHandler.Invoke(logLevel, eventId, message, args[0] as VerifyResponse);
                    return;
                }
            }
            else if (eventId == RecaptchaServiceEventId.VerifyResponseChecked)
            {
                if (_options.LogVerifyResponseCheckedMessageHandler != null)
                {
                    _options.LogVerifyResponseCheckedMessageHandler.Invoke(logLevel, eventId, message, args[0] as CheckResult);
                    return;
                }
            }
            else
            {
                throw CreateException($"Event {eventId} is missing", eventId);
            }

            _logger.Log(logLevel, eventId, e, message, args);
        }

        private RecaptchaLoggerException CreateException(string message, EventId currentEventId)
        {
            var se = new RecaptchaLoggerException(message);
            if (_options.EnableExceptionLogging && currentEventId != RecaptchaServiceEventId.ServiceException)
            {
                Log(RecaptchaServiceEventId.ServiceException, se);
            }
            return se;
        }

        public void Log(EventId eventId, params object[] args) => Log(eventId, null, args);
    }
}
