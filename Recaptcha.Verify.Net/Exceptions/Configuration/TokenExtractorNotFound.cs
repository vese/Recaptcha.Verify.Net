namespace Recaptcha.Verify.Net.Exceptions.Configuration;

/// <summary>
/// This exception is thrown when no <see cref="IRecaptchaTokenExtractor"/> implementation is registered in DI.
/// </summary>
public class TokenExtractorNotFound() : RecaptchaServiceConfigurationException("Requires at least one implementation of ITokenExtractor to be registered.") { }
