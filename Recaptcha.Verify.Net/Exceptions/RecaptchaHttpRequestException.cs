using System;
using System.Runtime.Serialization;

namespace Recaptcha.Verify.Net.Exceptions
{
    /// <summary>
    /// This exception is thrown when http request failed.
    /// Stores inner exception.
    /// </summary>
    [Serializable]
    public class RecaptchaHttpRequestException : RecaptchaServiceException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecaptchaHttpRequestException"/> class 
        /// with referense to the <see cref="ApiException"/>.
        /// </summary>
        public RecaptchaHttpRequestException(Exception inner) : base(inner.Message, inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecaptchaHttpRequestException"/> class with serialized data.
        /// </summary>
        protected RecaptchaHttpRequestException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
