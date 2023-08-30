using Microsoft.Extensions.Logging;

namespace Recaptcha.Verify.Net.Logging
{
    /// <summary>
    /// Event IDs for events that correspond to messages logged to an <see cref="ILogger" />.
    /// </summary>
    public static class CoreEventId
    {
        private const int _configurationBaseId = 10000;
        private const int _configurationErrorBaseId = _configurationBaseId + 1000;

        private const int _verifyRequestBaseId = 20000;
        private const int _verifyRequestErrorBaseId = _verifyRequestBaseId + 1000;

        private const int _checkResponseBaseId = 30000;
        private const int _checkResponseErrorBaseId = _checkResponseBaseId + 1000;

        private enum Id
        {
            // Configuration events
            MissingSecretKeyError = _configurationErrorBaseId,
            MissingActionError,
            MissingMinScoreError,

            // Verify request events
            SendingRequest = _verifyRequestBaseId,
            RequestCompleted,
            MissingCaptchaAnswerError = _verifyRequestErrorBaseId,
            HttpRequestError,

            // Check verify request response events
            ResponseChecked = _checkResponseBaseId,
        }

        private static EventId MakeId(Id id) => new EventId((int)id);

        /// <summary>
        /// Missing a secret key.
        /// </summary>
        public static readonly EventId MissingSecretKeyError = MakeId(Id.MissingSecretKeyError);
        /// <summary>
        /// Missing an action for verify V3 captcha request.
        /// </summary>
        public static readonly EventId MissingActionError = MakeId(Id.MissingActionError);
        /// <summary>
        /// Missing min score threshold for verify V3 captcha request.
        /// </summary>
        public static readonly EventId MissingMinScoreError = MakeId(Id.MissingMinScoreError);

        /// <summary>
        /// Sending verify request.
        /// </summary>
        public static readonly EventId SendingRequest = MakeId(Id.SendingRequest);
        /// <summary>
        /// Verify request completed.
        /// </summary>
        public static readonly EventId RequestCompleted = MakeId(Id.RequestCompleted);
        /// <summary>
        /// An empty captcha answer was received.
        /// </summary>
        public static readonly EventId MissingCaptchaAnswerError = MakeId(Id.MissingCaptchaAnswerError);
        /// <summary>
        /// An error occurred while processing verify request.
        /// </summary>
        public static readonly EventId HttpRequestError = MakeId(Id.HttpRequestError);

        /// <summary>
        /// Verify request response was checked.
        /// </summary>
        public static readonly EventId ResponseChecked = MakeId(Id.ResponseChecked);
    }
}
