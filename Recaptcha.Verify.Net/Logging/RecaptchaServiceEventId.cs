using Microsoft.Extensions.Logging;

namespace Recaptcha.Verify.Net
{
    public static class RecaptchaServiceEventId
    {
        private const string _prefix = "Recaptcha.Verify.Net__";

        private enum Id
        {
            Error = 100,
            Trace = 200,
        }

        public static readonly EventId Error = Id.Error.ToEventId();
        public static readonly EventId Trace = Id.Trace.ToEventId();

        private static EventId ToEventId(this Id id) => new EventId((int)id, _prefix + id);
    }
}
