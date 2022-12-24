using Microsoft.Extensions.Logging;
using System;

namespace Recaptcha.Verify.Net.Logging
{
    public interface IRecaptchaLoggerService
    {
        void Log(EventId id, Exception e, params object[] args);
        void Log(EventId id, params object[] args);
    }
}
