using Recaptcha.Verify.Net.TokenExtraction;

namespace Recaptcha.Verify.Net.Exceptions.Configuration;

/// <summary>
/// This exception is thrown when no <see cref="ITokenExtractor"/> implementation is registered in DI.
/// </summary>
public class TokenExtractorNotFound() : RecaptchaServiceConfigurationException("Requires at least one implementation of ITokenExtractor to be registered.") { }
