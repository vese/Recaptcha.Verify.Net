namespace Recaptcha.Verify.Net.Exceptions.Configuration;

/// <summary>
/// This exception is thrown when the action passed in function is empty.
/// </summary>
public class EmptyActionException() : RecaptchaServiceConfigurationException("Provided action is empty.") { }
