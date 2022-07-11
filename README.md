# Recaptcha.Verify.Net
[![NuGet](https://img.shields.io/nuget/v/Recaptcha.Verify.Net.svg)](https://www.nuget.org/packages/Recaptcha.Verify.Net) [![Build](https://github.com/vese/Recaptcha.Verify.Net/actions/workflows/build.yml/badge.svg?branch=master&event=push)](https://github.com/vese/Recaptcha.Verify.Net/actions/workflows/build.yml)

Library for server-side verification of Google reCAPTCHA v2/v3 response token for ASP.NET.

Recaptcha.Verify.Net starting from version 2.0.0 supports the following platforms and any .NET Standard 2.0 target:
- .NET Standard 2.0+
- .NET Framework 4.6.1+
- .NET Core 2.0+
- .NET 5+

# Table of Contents

- [Installation](#installation)
- [Verifying reCAPTCHA response](#verifying-recaptcha-response)
- [Using attribute for verifying reCAPTCHA response](#using-attribute-for-verifying-recaptcha-response)
- [Directly passing score threshold](#directly-passing-score-threshold)
- [Using score threshold map](#using-score-threshold-map)
- [Verifying reCAPTCHA response without checking action and score](#verifying-recaptcha-response-without-checking-action-and-score)
- [Handling exceptions](#handling-exceptions)
- [Examples](#examples)

### Installation
Package can be installed using Visual Studio UI (Tools > NuGet Package Manager > Manage NuGet Packages for Solution and search for "Recaptcha.Verify.Net").

Also latest version of package can be installed using Package Manager Console:
```
PM> Install-Package Recaptcha.Verify.Net
```

### Verifying reCAPTCHA response
1. Add secret key in appsettings.json file.
```json
{
  "Recaptcha": {
    "SecretKey": "<recaptcha secret key>",
    "ScoreThreshold": 0.5
  }
}
```
2. Configure service in Startup.cs.
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.ConfigureRecaptcha(Configuration.GetSection("Recaptcha"));
    //...
}
```
3. Use service in controller to verify captcha answer and check response for V3 action and score.
```csharp
[ApiController]
[Route("api/[controller]")]
public class LoginController : Controller
{
    private const string _loginAction = "login";
    
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
        var checkResult = await _recaptchaService.VerifyAndCheckAsync(
            credentials.RecaptchaToken,
            _loginAction,
            cancellationToken);
        
        if (!checkResult.Success)
        {
            if (!checkResult.Response.Success)
            {
                // Handle unsuccessful verification response
                _logger.LogError($"Recaptcha error: {JsonConvert.SerializeObject(checkResult.Response.ErrorCodes)}");
            }
            
            if (!checkResult.ScoreSatisfies)
            {
                // Handle score less than specified threshold for v3
            }
            
            // Unsuccessful verification and check
            return BadRequest();
        }
        
        // Process login
        
        return Ok();
    }
}
```
### Using attribute for verifying reCAPTCHA response
1. Specify in appsettings.json name of parameter for a way in which reCAPTCHA response token is passed.
```json
{
  "Recaptcha": {
    ...
    "AttributeOptions": {
      "ResponseTokenNameInHeader": "RecaptchaTokenInHeader", // If token is passed in header
      "ResponseTokenNameInQuery": "RecaptchaTokenInQuery", // If token is passed in query
      "ResponseTokenNameInForm": "RecaptchaTokenInForm" // If token is passed in form
    }
  }
}
```
Or set in Startup GetResponseTokenFromActionArguments or GetResponseTokenFromExecutingContext delegate that points how to get token from parsed data.
```csharp
services.ConfigureRecaptcha(Configuration.GetSection("Recaptcha"),
    // Specify how to get token from parsed arguments for using in RecaptchaAttribute
    o => o.AttributeOptions.GetResponseTokenFromActionArguments =
        d =>
        { 
            if (d.TryGetValue("credentials", out var credentials))
            {
                return ((BaseRecaptchaCredentials)credentials).RecaptchaToken;
            }
            return null;
        });
```
Credentials model used in example has base class with property containing token. 
```csharp
public class BaseRecaptchaCredentials
{
    public string RecaptchaToken { get; set; }
}
public class Credentials : BaseRecaptchaCredentials
{
    public string Login { get; set; }
    public string Password { get; set; }
}
```
2. Add Recaptcha attribute in controller to verify captcha answer and check response action and score (score for V3).
```csharp
[Recaptcha("login")]
[HttpPost("Login")]
public async Task<IActionResult> Login([FromBody] Credentials credentials, CancellationToken cancellationToken)
{
    // Process login
    return Ok();
}
```
### Directly passing score threshold
Score threshold in appsettings.json is optional and value could be passed directly into VerifyAndCheckAsync function.
```csharp
var scoreThreshold = 0.5f;
var checkResult = await _recaptchaService.VerifyAndCheckAsync(
    credentials.RecaptchaToken,
    _loginAction,
    scoreThreshold);
```
### Using score threshold map
Based on the score, you can take variable action in the context of your site instead of blocking traffic to better protect your site. Score thresholds specified for actions allow you to achieve adaptive risk analysis and protection based on the context of the action.
1. Specify ActionsScoreThresholds in appsettings.json. If specified ScoreThreshold value will be used as default score threshold for actions that are not in map.
```json
{
  "Recaptcha": {
    "SecretKey": "<recaptcha secret key>",
    "ScoreThreshold": 0.5,
    "ActionsScoreThresholds": {
      "login": 0.75,
      "test": 0.9
    }
  }
}
```
2. Call VerifyAndCheckAsync function
```csharp
// Response will be checked with score threshold equal to 0.75
var checkResultLogin  = await _recaptchaService.VerifyAndCheckAsync(credentials.RecaptchaToken, "login");

// Response will be checked with score threshold equal to 0.9
var checkResultTest   = await _recaptchaService.VerifyAndCheckAsync(credentials.RecaptchaToken, "test");

// Response will be checked with score threshold equal to 0.5
var checkResultSignUp = await _recaptchaService.VerifyAndCheckAsync(credentials.RecaptchaToken, "signup");
```
### Verifying reCAPTCHA response without checking action and score
If checking of verification response needs to be completed separately then you can use VerifyAsync insted of VerifyAndCheckAsync.
```csharp
var response = await _recaptchaService.VerifyAsync(credentials.RecaptchaToken);
```
### Handling exceptions
Library can produce following exceptions
Exception | Description
--- | ---
EmptyActionException | This exception is thrown when the action passed in function is empty.
EmptyCaptchaAnswerException | This exception is thrown when captcha answer passed in function is empty.
HttpRequestException | This exception is thrown when http request failed. Stores Refit.ApiException as inner exception.
MinScoreNotSpecifiedException | This exception is thrown when minimal score was not specified and request had score value (used V3 reCAPTCHA).
SecretKeyNotSpecifiedException | This exception is thrown when secret key was not specified in options or request params.
UnknownErrorKeyException | This exception is thrown when verification response error key is unknown.

All of these exceptions are inherited from RecaptchaServiceException.
### Examples
Examples could be found in library repository:
- [**Recaptcha.Verify.Net.ConsoleApp**](https://github.com/vese/Recaptcha.Verify.Net/blob/master/examples/Recaptcha.Verify.Net.ConsoleApp/Program.cs "Link") (.NET Core 3.1)
- [**Recaptcha.Verify.Net.AspNetCoreAngular**](https://github.com/vese/Recaptcha.Verify.Net/blob/master/examples/Recaptcha.Verify.Net.AspNetCoreAngular/Controllers/LoginController.cs "Link") (ASP.NET Core 3.1 + Angular)
