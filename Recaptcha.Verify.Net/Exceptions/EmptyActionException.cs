using System;
using System.Runtime.Serialization;

namespace Recaptcha.Verify.Net.Exceptions
{
    /// <summary>
    /// This exception is thrown when the action passed in function is empty.
    /// </summary>
    [Serializable]
    public class EmptyActionException : RecaptchaServiceException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyActionException"/> class.
        /// </summary>
        public EmptyActionException() : base("Provided action is empty.") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyActionException"/> class with serialized data.
        /// </summary>
        protected EmptyActionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
