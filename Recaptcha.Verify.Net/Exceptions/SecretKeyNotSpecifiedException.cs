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
        /// <summary>
        /// Initializes a new instance of the <see cref="SecretKeyNotSpecifiedException"/> class.
        /// </summary>
        public SecretKeyNotSpecifiedException(): base("Secret key was not provided.") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretKeyNotSpecifiedException"/> class with serialized data.
        /// </summary>
        protected SecretKeyNotSpecifiedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
