namespace Recaptcha.Verify.Net.Exceptions.Processing;

/// <summary>
/// This exception is thrown when captcha answer is empty.
/// </summary>
public class EmptyCaptchaAnswerException() : RecaptchaServiceProcessingException("Received captcha answer is empty.") { }
