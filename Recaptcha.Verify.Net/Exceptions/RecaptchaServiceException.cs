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
        /// <summary>
        /// Initializes a new instance of the <see cref="RecaptchaServiceException"/> class.
        /// </summary>
        public RecaptchaServiceException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecaptchaServiceException"/> class 
        /// with a specified error message.
        /// </summary>
        public RecaptchaServiceException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecaptchaServiceException"/> class with a specified error
        /// message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        public RecaptchaServiceException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecaptchaServiceException"/> class with serialized data.
        /// </summary>
        protected RecaptchaServiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
