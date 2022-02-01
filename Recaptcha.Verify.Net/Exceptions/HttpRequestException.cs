using Refit;
using System;
using System.Runtime.Serialization;

namespace Recaptcha.Verify.Net.Exceptions
{
    /// <summary>
    /// This exception is thrown when http request failed.
    /// Stores <see cref="Refit.ApiException"/> as inner exception.
    /// </summary>
    [Serializable]
    public class HttpRequestException : RecaptchaServiceException
    {
        public HttpRequestException(ApiException inner) : base(inner.Message, inner) { }
        public HttpRequestException(string message, ApiException inner) : base(message, inner) { }
        protected HttpRequestException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
