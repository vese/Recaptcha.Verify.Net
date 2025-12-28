namespace Recaptcha.Verify.Net.Exceptions.Processing;

/// <summary>
/// This exception is thrown when verification response error key is unknown.
/// </summary>
/// <param name="key">The key of the error.</param>
public class UnknownErrorKeyException(string key) : RecaptchaServiceProcessingException($"Unknown error key: {key}.")
{
    /// <summary>
    /// The key of the error.
    /// </summary>
    public string Key { get; } = key;
}
