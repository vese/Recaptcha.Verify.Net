using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recaptcha.Verify.Net.Configuration;
using System.Collections.Generic;

namespace Recaptcha.Verify.Net.Logging
{
    internal class RecaptchaLoggerService : IRecaptchaLoggerService
    {
        private static readonly Dictionary<EventId, string> _eventsLogMessageTemplates = new Dictionary<EventId, string>()
        {
            { RecaptchaServiceEventId.Error, "Error" },
            { RecaptchaServiceEventId.Trace, "Trace" },
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

        public void Log(EventId eventId, params object[] args)
        {
            if (!_options.EnableLogging)
            {
                return;
            }

            if (!_options.LogLevels.TryGetValue(eventId, out var logLevel))
            {
                //throw new RecaptchaLoggerException($"Log level for event with id {id} is missing", args);
            }
            if (!_eventsLogMessageTemplates.TryGetValue(eventId, out var message))
            {
                //throw new RecaptchaLoggerException($"Message for event with id {id} is missing", args);
            }

            if (eventId == RecaptchaServiceEventId.Error)
            {
                if (_options.LogErrorMessageHandler != null)
                {
                    _options.LogErrorMessageHandler.Invoke(logLevel, eventId, message, args);
                    return;
                }
            }
            else if (eventId == RecaptchaServiceEventId.Trace)
            {
                if (_options.LogTraceMessageHandler != null)
                {
                    _options.LogTraceMessageHandler.Invoke(logLevel, eventId, message, args);
                    return;
                }
            }
            else
            {
                //throw new RecaptchaLoggerException($"Event with id {id} is missing", args);
            }

            _logger.Log(logLevel, eventId, message, args);
        }
    }
}
