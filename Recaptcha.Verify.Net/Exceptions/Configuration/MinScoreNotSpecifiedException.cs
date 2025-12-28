namespace Recaptcha.Verify.Net.Exceptions.Configuration;

/// <summary>
/// This exception is thrown when minimal score was not specified and request had score value.
/// </summary>
/// <param name="action">The action.</param>
public class MinScoreNotSpecifiedException(string action) : RecaptchaServiceConfigurationException($"Score threshold was not provided for action {action}.")
{
    /// <summary>
    /// The action.
    /// </summary>
    public string Action { get; } = action;
}
