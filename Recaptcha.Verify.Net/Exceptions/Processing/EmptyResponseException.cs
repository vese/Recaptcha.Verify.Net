namespace Recaptcha.Verify.Net.Exceptions.Processing;

/// <summary>
/// This exception is thrown when http request answer is empty.
/// </summary>
public class EmptyResponseException() : RecaptchaServiceProcessingException("Verifivation response is empty") { }
