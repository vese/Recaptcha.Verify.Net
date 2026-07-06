# Recaptcha.Verify.Net
[![NuGet](https://img.shields.io/nuget/v/Recaptcha.Verify.Net.svg)](https://www.nuget.org/packages/Recaptcha.Verify.Net)
[![Build 3.1](https://github.com/vese/Recaptcha.Verify.Net/actions/workflows/build.yml/badge.svg?branch=release/v3.1&event=push)](https://github.com/vese/Recaptcha.Verify.Net/actions/workflows/build.yml?query=branch%3Arelease%2Fv3.1)

A lightweight library for server-side verification of Google reCAPTCHA v2 and v3 response tokens in .NET applications.

Starting with version 3.0.0, Recaptcha.Verify.Net supports the following platforms:
- .NET 8
- .NET 9
- .NET 10
- .NET 11

For .NET Framework or for other older .NET versions, please use a version below 3.0.0.

# Table of Contents

- [Installation](#installation)
- [Basic Usage](#basic-usage)
- [Verification Process](#verification-process)
- [Configuring Actions and Score Thresholds](#configuring-actions-and-score-thresholds)
- [Custom Verification URL](#custom-verification-url)
- [Response Customization](#response-customization)
- [Handling Exceptions](#handling-exceptions)
- [Examples](#examples)

## Installation
The package is available on [NuGet](https://www.nuget.org/packages/Recaptcha.Verify.Net) and can be installed through your preferred method.

- Via Package Manager Console
   ```powershell
   PM> Install-Package Recaptcha.Verify.Net -Version 3.0.1
   ```
- Via .NET CLI
   ```bash
   dotnet add package Recaptcha.Verify.Net --version 3.0.1
   ```
- Adding reference to project file
   ```xml
   <PackageReference Include="Recaptcha.Verify.Net" Version="3.0.1" />
   ```

## Basic Usage
1. Add settings to appsettings.json. Specify reCAPTCHA secret token and a way to receive reCAPTCHA response token from request.
   ```json
   {
     "Recaptcha": {
       "Verification": {
         "SecretKey": "<recaptcha secret key>"
       },
       "TokenExtractors": {
         "Header": "X-Recaptcha-Token"
       }
     }
   }
   ```
1. Register library services for DI.
   ```csharp
   services.AddRecaptcha(configuration.GetSection("Recaptcha"));
   ```
1. Apply the `RecaptchaAttribute` to protected endpoints. Specify action and score threshold for validating v3 verification result.
   ```csharp
   [Recaptcha("login", 0.5f)]
   [HttpPost("login")]
   public async Task<IActionResult> Login([FromBody] Credentials credentials, CancellationToken cancellationToken)
   {
       // Process login
       return Ok();
   }
   ```

## Verification Process
The verification of a reCAPTCHA response token consists of three sequential steps:
1. Extracting the token from the request
1. Verifying the token with Google's API
1. Validating the verification result

### Extracting reCAPTCHA Response Token
Token extraction is coordinated by the `RecaptchaTokenExtractionService`, which iterates the registered `IRecaptchaTokenExtractor` implementations and returns the first non-empty token (first-wins strategy).

#### Built-in Extractors
The library provides these extractors out of the box:

|Extractor|Description|Registration Method|
|---|---|---|
|`FormTokenExtractor`|Extracts token from request form data|`services.AddRecaptchaFormTokenExtractor(name)`|
|`HeaderTokenExtractor`|Extracts token from request headers|`services.AddRecaptchaHeaderTokenExtractor(name)`|
|`QueryTokenExtractor`|Extracts token from query parameters|`services.AddRecaptchaQueryTokenExtractor(name)`|
|`ActionArgumentsTokenExtractor`|Extracts token from parsed action arguments|`services.AddRecaptchaActionArgumentsTokenExtractor(name)` or `services.AddRecaptchaActionArgumentsTokenExtractor(delegate)`|
|`ExecutingContextTokenExtractor`|Extracts token from the executing context|`services.AddRecaptchaExecutingContextTokenExtractor(delegate)`|

#### Auto-registration via Configuration
When using `services.AddRecaptcha()`, extractors are automatically registered based on provided configuration.
Configure token extractors in `RecaptchaOptions.TokenExtractors`:
- if `TokenExtractors.Header` is not empty then registers `HeaderTokenExtractor`;
- if `TokenExtractors.Form` is not empty then registers `FormTokenExtractor`;
- if `TokenExtractors.Query` is not empty then registers `QueryTokenExtractor` (not recommended for security reasons);
- if `TokenExtractors.ActionArgument` is not empty then registers `ActionArgumentsTokenExtractor`.

For delegate-based extractors that cannot be configured via JSON, set them in code on `RecaptchaOptions.TokenExtractors`:
- if `TokenExtractors.GetResponseTokenFromActionArguments` delegate is not empty then registers `ActionArgumentsTokenExtractor`;
- if `TokenExtractors.GetResponseTokenFromExecutingContext` delegate is not empty then registers `ExecutingContextTokenExtractor`.

#### Important Notes
- Query Parameters: Avoid using `QueryTokenExtractor`, as tokens in URLs may be logged or cached. Prefer headers or the request body.
- Request Body: For tokens sent in the request body, use `ActionArgumentsTokenExtractor`. By the time the attribute runs, model binding has already parsed the body into the controller method arguments, so the token can be read from a bound argument (or a property of a bound model) without re-reading the request stream. To read raw fields from form-encoded data instead, use `FormTokenExtractor` (`Request.Form`).
- Custom Logic: For complex extraction scenarios, either:
  - Use `ExecutingContextTokenExtractor` with delegate;
  - Implement your own `IRecaptchaTokenExtractor` and register it in the DI container.

#### Manual Registration Example
```csharp
services.AddRecaptchaHeaderTokenExtractor("X-Recaptcha-Token");
services.AddRecaptchaActionArgumentsTokenExtractor(args => 
    args.TryGetValue("recaptchaToken", out var token) ? token?.ToString() : null);
```

### Token Verification
Token verification is performed by the `RecaptchaVerificationService`, which communicates with Google's reCAPTCHA API through the `RecaptchaClient`.
The service returns a `VerifyResponse` object containing verification details.

#### `VerifyResponse` Properties
|Property|Type|Description|
|---|---|---|
|`Success`|`bool`|Indicates whether this request was a valid reCAPTCHA token for your site|
|`IsV3`|`bool`|Indicates whether this is a reCAPTCHA v3 verification (based on score presence)|
|`Score`|`float?`|**reCAPTCHA v3 only**: Confidence score from 0.0 (likely bot) to 1.0 (likely human)|
|`Action`|`string?`|**reCAPTCHA v3 only**: The action name specified during client-side execution|
|`ChallengeTs`|`DateTime`|Timestamp when the challenge was loaded|
|`Hostname`|`string?`|The hostname of the site where the reCAPTCHA was solved|
|`ApkPackageName`|`string?`|For Android applications: the package name of the APK|
|`ErrorCodes`|`IReadOnlyCollection<string>?`|Raw error codes returned by Google's API|
|`Errors`|`IReadOnlyCollection<VerifyError>?`|Parsed error codes as VerifyError enum values|

#### Domain/Package Name Validation
By default, Google validates that the Hostname or ApkPackageName matches your registered domains/packages.
This security feature can be disabled in the reCAPTCHA admin console if needed.

#### References
[Verifying the user's response](https://developers.google.com/recaptcha/docs/verify)  
[reCAPTCHA v3 Actions](https://developers.google.com/recaptcha/docs/v3#actions)  
[reCAPTCHA v3 Site Verify Response](https://developers.google.com/recaptcha/docs/v3#site_verify_response)  
[Domain/Package Name Validation](https://developers.google.com/recaptcha/docs/domain_validation)

### Validating the verification result
After token verification, the result must be validated.
This is performed by the `RecaptchaVerificationResultValidationService`.

#### Validation Logic
For reCAPTCHA v3 (detected by the presence of a Score value), the service checks:
- **Action Matching**: Ensures the returned action matches the expected value
  - Provided in the `RecaptchaAttribute` constructor
  - Global action `RecaptchaOptions.Validation.Action`
- **Score Threshold**: Ensures the confidence score meets the required threshold
  - Provided in the `RecaptchaAttribute` constructor
  - Global threshold `RecaptchaOptions.Validation.ScoreThreshold`
  - Action-specific threshold specified in `RecaptchaOptions.Validation.ActionsScoreThresholds` dictionary

The service returns a `ValidationResult` object with detailed validation status.

#### `ValidationResult` Properties
|Property|Type|Description|
|---|---|---|
|`ResponseSuccessful`|`bool`|Indicates whether verify request was successful|
|`IsV3`|`bool`|Indicates whether this is a reCAPTCHA v3 verification (based on score presence)|
|`ActionMatches`|`bool`|Indicates whether the action matches the expected value|
|`ScoreSatisfies`|`bool`|Indicates whether the score meets the required threshold|
|`Success`|`bool`|**Overall validation result** — `true` only when all applicable checks pass|

## Configuring Actions and Score Thresholds
For reCAPTCHA v3 validation, both an action and a score threshold must be specified.
These can be configured either:
- Per-endpoint via the `RecaptchaAttribute` constructor
- Globally via RecaptchaOptions

### Configuration Hierarchy
The system follows this priority order (highest to lowest):

**For Actions**:
1. `RecaptchaAttribute` constructor value
1. `RecaptchaOptions.Validation.Action` - global default action for all v3 validations

**For Score Thresholds**:
- `RecaptchaAttribute` constructor value
- `RecaptchaOptions.Validation.ActionsScoreThresholds[action]` - action-specific thresholds map
- `RecaptchaOptions.Validation.ScoreThreshold` - global default score threshold (By default, you can use a threshold of 0.5)

### Configuring Actions and Score Thresholds Example
```json
{
  "Recaptcha": {
    "Validation": {
      "Action": "default_action",
      "ScoreThreshold": 0.5,
      "ActionsScoreThresholds": {
        "login": 0.8,
        "comment": 0.3
      }
    }
  }
}
```
```csharp
// Uses global action "default_action" and threshold 0.5
[Recaptcha]
public IActionResult DefaultEndpoint() { ... }

// Uses action "register" with global threshold 0.5
[Recaptcha("register")]
public IActionResult Register() { ... }

// Uses action "login" with threshold from ActionsScoreThresholds (0.8)
[Recaptcha("login")]
public IActionResult Login() { ... }

// Uses action "comment" with explicit threshold 0.4 (overrides the map)
[Recaptcha("comment", 0.4)]
public IActionResult AddComment() { ... }
```

## Custom Verification URL
By default, the library communicates with Google's reCAPTCHA API at `https://www.google.com/recaptcha/api`.
To use a custom endpoint (e.g. a mirror or proxy), set the `BaseUrl` option in `RecaptchaOptions.Verification`:

### Via appsettings.json
```json
{
  "Recaptcha": {
    "Verification": {
      "SecretKey": "<recaptcha secret key>",
      "BaseUrl": "https://recaptcha-proxy.example.com/recaptcha/api"
    },
    "TokenExtractors": {
      "Header": "X-Recaptcha-Token"
    }
  }
}
```

### Via code configuration
```csharp
services.AddRecaptcha(o =>
{
    o.Verification.SecretKey = "<recaptcha secret key>";
    o.Verification.BaseUrl = "https://recaptcha-proxy.example.com/recaptcha/api";
});
```

When `BaseUrl` is not specified, the default Google endpoint is used.

## Response Customization
When reCAPTCHA verification fails, the library returns a `400 Bad Request` response by default.
Message specified in `RecaptchaOptions.Attribute.VerificationFailedMessage` which default value is "Recaptcha verification failed".

You can customize the response by setting the `RecaptchaOptions.Attribute.OnVerificationFailed` delegate, which is called when token verification or validation fails.
The `IActionResult` returned by this delegate is used as the HTTP response.
Delegate may also throw exceptions, which will not be caught by `RecaptchaAttribute`.

When an exception occurs during verification, a `RecaptchaServiceException`-derived exception is thrown (see [Handling Exceptions](#handling-exceptions) for the full list).
These exceptions are not caught by `RecaptchaAttribute` and propagate up the ASP.NET request pipeline, where they are handled according to the application's configured exception handling rules.
See the [ASP.NET Core example](https://github.com/vese/Recaptcha.Verify.Net/tree/release/v3.1/examples/Recaptcha.Verify.Net.AspNetCoreAngular) for a reference implementation.

## Handling Exceptions
Library can produce following exceptions
Exception | Description
--- | ---
`RecaptchaServiceException` | Base recaptcha exception. Other exceptions are inherited from this.
`RecaptchaServiceConfigurationException` | Base recaptcha exception for invalid configuration.
`EmptyActionException` | This exception is thrown when the action passed in function is empty.
`MinScoreNotSpecifiedException` | This exception is thrown when minimal score was not specified and request had score value (used V3 reCAPTCHA).
`SecretKeyNotSpecifiedException` | This exception is thrown when secret key was not specified in options or request params.
`TokenExtractorNotFound` | This exception is thrown when no ITokenExtractor implementation is registered in DI.
`RecaptchaServiceProcessingException` | Base recaptcha exception for errors while processing token verification and result verification.
`EmptyCaptchaAnswerException` | This exception is thrown when captcha answer passed in function is empty.
`EmptyResponseException` | This exception is thrown when verification request response is empty. When thrown, it is wrapped in VerifyRequestException.
`VerifyRequestException` | This exception is thrown when verification request failed. Stores inner exception.
`RecaptchaUnknownException` | This exception is thrown when an unexpected exception is catched during processing captcha.

## Examples
Examples could be found in library repository:
- [**Recaptcha.Verify.Net.ConsoleApp**](https://github.com/vese/Recaptcha.Verify.Net/blob/release/v3.1/examples/Recaptcha.Verify.Net.ConsoleApp/Program.cs) (.NET 10)
- [**Recaptcha.Verify.Net.AspNetCoreAngular**](https://github.com/vese/Recaptcha.Verify.Net/tree/release/v3.1/examples/Recaptcha.Verify.Net.AspNetCoreAngular) (ASP.NET + Angular)
