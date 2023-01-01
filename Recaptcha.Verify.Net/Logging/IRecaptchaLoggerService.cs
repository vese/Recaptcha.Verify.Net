using Microsoft.Extensions.Logging;

namespace Recaptcha.Verify.Net.Logging
{
    internal interface IRecaptchaLoggerService
    {
        void Log(EventId id, params object[] args);
    }
}
