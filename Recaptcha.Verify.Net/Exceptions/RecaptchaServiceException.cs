using System;
using System.Runtime.Serialization;

namespace Recaptcha.Verify.Net.Exceptions
{
    /// <summary>
    /// Base Recaptcha service exception.
    /// </summary>
    [Serializable]
    public class RecaptchaServiceException : Exception
    {
        public RecaptchaServiceException() { }
        public RecaptchaServiceException(string message) : base(message) { }
        public RecaptchaServiceException(string message, Exception inner) : base(message, inner) { }
        protected RecaptchaServiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
