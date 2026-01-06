namespace Recaptcha.Verify.Net.AspNetCoreAngular.Server.Models;

public class Credentials : BaseRecaptchaCredentials
{
    public required string Login { get; init; }
    public required string Password { get; init; }
}
