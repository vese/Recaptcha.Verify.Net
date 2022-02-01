using System;
using System.Runtime.Serialization;

namespace Recaptcha.Verify.Net.Exceptions
{
    /// <summary>
    /// This exception is thrown when verification response error key is unknown.
    /// </summary>
    [Serializable]
    public class UnknownErrorKeyException : RecaptchaServiceException
    {
        public UnknownErrorKeyException() { }
        public UnknownErrorKeyException(string message) : base(message) { }
        public UnknownErrorKeyException(string message, Exception inner) : base(message, inner) { }
        protected UnknownErrorKeyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
