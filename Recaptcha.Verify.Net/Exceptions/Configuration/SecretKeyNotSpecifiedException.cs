namespace Recaptcha.Verify.Net.Exceptions.Configuration;

/// <summary>
/// This exception is thrown when secret key was not specified in options or request params.
/// </summary>
public class SecretKeyNotSpecifiedException() : RecaptchaServiceConfigurationException("Secret key was not provided.") { }
