using System;
using System.Runtime.Serialization;

namespace Recaptcha.Verify.Net.Exceptions
{
    /// <summary>
    /// This exception is thrown when minimal score was not specified and request had score value.
    /// </summary>
    [Serializable]
    public class MinScoreNotSpecifiedException : RecaptchaServiceException
    {
        public MinScoreNotSpecifiedException() { }
        public MinScoreNotSpecifiedException(string message) : base(message) { }
        public MinScoreNotSpecifiedException(string message, Exception inner) : base(message, inner) { }
        protected MinScoreNotSpecifiedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
