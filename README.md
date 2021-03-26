# Recaptcha.Verify.Net
Library for verifying Google ReCaptcha v2/v3 response token in ASP.NET Core 3.1+

||Recaptcha.Verify.Net|
| :------------: | :------------: |
|*NuGet*|[![NuGet](https://img.shields.io/nuget/v/Refit.svg)](https://img.shields.io/nuget/v/Recaptcha.Verify.Net.svg)|

### Using reCAPTCHA verification
1. Add secret key in appsettings
```json
{
  "Recaptcha": {
    "SecretKey": "<recaptcha secret key>"
  }
}
```
2. Configure service in Startup.cs
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.ConfigureRecaptcha(Configuration.GetSection("Recaptcha"));
    //...
}
```
3. Use in controller
```csharp
[ApiController]
[Route("api/[controller]")]
public class LoginController : Controller
{
    private readonly ILogger _logger;
    private readonly IRecaptchaService _recaptchaService;

    public LoginController(ILoggerFactory loggerFactory, IRecaptchaService recaptchaService)
    {
        _logger = loggerFactory.CreateLogger<LoginController>();
        _recaptchaService = recaptchaService;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] Credentials credentials, CancellationToken cancellationToken)
    {
        var response = await _recaptchaService.VerifyAsync(
            new VerifyRequest()
            {
                Response = credentials.RecaptchaToken
            },
            cancellationToken);

        if (!response.Success)
        {
            _logger.LogError($"Recaptcha error: {JsonConvert.SerializeObject(response.ErrorCodes)}");
            return BadRequest();
        }

        // Process login

        return Ok();
    }
}
```
