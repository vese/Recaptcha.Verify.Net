namespace Recaptcha.Verify.Net.Exceptions.Processing;

/// <summary>
/// Base Recaptcha service exception.
/// </summary>
public class RecaptchaServiceProcessingException : RecaptchaServiceException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecaptchaServiceException"/> class.
    /// </summary>
    public RecaptchaServiceProcessingException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecaptchaServiceException"/> class 
    /// with a specified error message.
    /// </summary>
    public RecaptchaServiceProcessingException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecaptchaServiceException"/> class with a specified error
    /// message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    public RecaptchaServiceProcessingException(string message, Exception inner) : base(message, inner) { }
}
