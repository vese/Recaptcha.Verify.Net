namespace Recaptcha.Verify.Net.AspNetCoreAngular.Models
{
    public class Credentials : BaseRecaptchaCredentials
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
