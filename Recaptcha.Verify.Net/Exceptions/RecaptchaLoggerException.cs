using System;
using System.Runtime.Serialization;

namespace Recaptcha.Verify.Net.Exceptions
{
    /// <summary>
    /// Recaptcha logger service exception.
    /// </summary>
    [Serializable]
    public class RecaptchaLoggerException : RecaptchaServiceException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecaptchaLoggerException"/> class 
        /// with a specified error message.
        /// </summary>
        public RecaptchaLoggerException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecaptchaLoggerException"/> class with serialized data.
        /// </summary>
        protected RecaptchaLoggerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
