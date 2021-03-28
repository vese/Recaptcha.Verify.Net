using System;
using System.Runtime.Serialization;

namespace Recaptcha.Verify.Net.Exceptions
{
    /// <summary>
    /// This exception is thrown when captcha answer is empty.
    /// </summary>
    [Serializable]
    public class EmptyCaptchaAnswerException : RecaptchaServiceException
    {
        public EmptyCaptchaAnswerException() { }
        public EmptyCaptchaAnswerException(string message) : base(message) { }
        public EmptyCaptchaAnswerException(string message, Exception inner) : base(message, inner) { }
        protected EmptyCaptchaAnswerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
