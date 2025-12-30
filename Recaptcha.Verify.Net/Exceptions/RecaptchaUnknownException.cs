namespace Recaptcha.Verify.Net.Exceptions;

/// <summary>
/// This exception is thrown when an unexpected exception is catched during  processing captcha.
/// </summary>
/// <param name="e">Catched exception.</param>
public class RecaptchaUnknownException(Exception e) : RecaptchaServiceException(e.Message, e) { }
