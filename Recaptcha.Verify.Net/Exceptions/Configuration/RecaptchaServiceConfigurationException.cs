namespace Recaptcha.Verify.Net.Exceptions.Configuration;

/// <summary>
/// Base Recaptcha service exception.
/// </summary>
public class RecaptchaServiceConfigurationException : RecaptchaServiceException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecaptchaServiceException"/> class.
    /// </summary>
    public RecaptchaServiceConfigurationException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecaptchaServiceException"/> class 
    /// with a specified error message.
    /// </summary>
    public RecaptchaServiceConfigurationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecaptchaServiceException"/> class with a specified error
    /// message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    public RecaptchaServiceConfigurationException(string message, Exception inner) : base(message, inner) { }
}
