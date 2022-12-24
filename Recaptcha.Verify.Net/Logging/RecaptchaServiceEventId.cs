using Microsoft.Extensions.Logging;

namespace Recaptcha.Verify.Net
{
    public static class RecaptchaServiceEventId
    {
        private const string _prefix = "Recaptcha.Verify.Net__";

        private enum Id
        {
            ServiceException = 0,
            SendingRequest = 100,
            RequestSucceded,
            VerifyResponseChecked,
        }

        public static readonly EventId ServiceException = Id.ServiceException.ToEventId();
        public static readonly EventId SendingRequest = Id.SendingRequest.ToEventId();
        public static readonly EventId RequestSucceded = Id.RequestSucceded.ToEventId();
        public static readonly EventId VerifyResponseChecked = Id.VerifyResponseChecked.ToEventId();

        private static EventId ToEventId(this Id id) => new EventId((int)id, _prefix + id);
    }
}
