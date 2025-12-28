namespace Recaptcha.Verify.Net.Exceptions.Processing;

/// <summary>
/// This exception is thrown when http request failed.
/// Stores inner exception.
/// </summary>
public class VerifyRequestException(Exception inner) : RecaptchaServiceProcessingException(inner.Message, inner) { }
