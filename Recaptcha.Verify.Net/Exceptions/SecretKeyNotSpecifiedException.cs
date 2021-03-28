using System;
using System.Runtime.Serialization;

namespace Recaptcha.Verify.Net.Exceptions
{
    /// <summary>
    /// This exception is thrown when secret key was not specified in options or request params.
    /// </summary>
    [Serializable]
    public class SecretKeyNotSpecifiedException : RecaptchaServiceException
    {
        public SecretKeyNotSpecifiedException() { }
        public SecretKeyNotSpecifiedException(string message) : base(message) { }
        public SecretKeyNotSpecifiedException(string message, Exception inner) : base(message, inner) { }
        protected SecretKeyNotSpecifiedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
