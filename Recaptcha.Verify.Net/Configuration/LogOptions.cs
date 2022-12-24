using Microsoft.Extensions.Logging;
using Recaptcha.Verify.Net.Exceptions;
using System;
using System.Collections.Generic;

namespace Recaptcha.Verify.Net.Configuration
{
    public class LogOptions
    {
        internal readonly Dictionary<EventId, LogLevel> LogLevels = new Dictionary<EventId, LogLevel>
        {
            { RecaptchaServiceEventId.ServiceException, LogLevel.Error },
            { RecaptchaServiceEventId.SendingRequest, LogLevel.Trace },
            { RecaptchaServiceEventId.RequestSucceded, LogLevel.Debug },
            { RecaptchaServiceEventId.VerifyResponseChecked, LogLevel.Debug },
        };

        public bool EnableLogging { get; set; } = true;
        public bool EnableExceptionLogging { get; set; } = false;

        public Action<LogLevel, EventId, string, Exception> LogServiceExceptionMessageHandler { get; set; }
        public Action<LogLevel, EventId, string, string, string> LogSendingRequestMessageHandler { get; set; }
        public Action<LogLevel, EventId, string, VerifyResponse> LogRequestSuccededMessageHandler { get; set; }
        public Action<LogLevel, EventId, string, CheckResult> LogVerifyResponseCheckedMessageHandler { get; set; }

        public void UpdateLevels(params (EventId Id, LogLevel Level)[] eventsAndLevels)
        {
            foreach ((var eventId, var level) in eventsAndLevels)
            {
                if (LogLevels.ContainsKey(eventId))
                {
                    LogLevels[eventId] = level;
                }
                else
                {
                    throw new RecaptchaLoggerException($"Event {eventId} is missing");
                }
            }
        }
    }
}
