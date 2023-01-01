using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Recaptcha.Verify.Net.Configuration
{
    public class LogOptions
    {
        internal readonly Dictionary<EventId, LogLevel> LogLevels = new Dictionary<EventId, LogLevel>
        {
            { RecaptchaServiceEventId.Error, LogLevel.Error },
            { RecaptchaServiceEventId.Trace, LogLevel.Trace },
        };
        public bool EnableLogging { get; set; } = true;
        public Action<LogLevel, EventId, string, object[]> LogErrorMessageHandler { get; set; }
        public Action<LogLevel, EventId, string, object[]> LogTraceMessageHandler { get; set; }
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
                    // throw new ConfigurationError
                }
            }
        }
    }
}
