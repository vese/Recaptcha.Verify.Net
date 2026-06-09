# Recaptcha.Verify.Net
[![NuGet](https://img.shields.io/nuget/v/Recaptcha.Verify.Net.svg)](https://www.nuget.org/packages/Recaptcha.Verify.Net)
[![Build 3.0](https://github.com/vese/Recaptcha.Verify.Net/actions/workflows/build.yml/badge.svg?branch=release/v3.0&event=push)](https://github.com/vese/Recaptcha.Verify.Net/actions/workflows/build.yml?query=branch%3Arelease%2Fv3.0)

A lightweight library for server-side verification of Google reCAPTCHA v2 and v3 response tokens in .NET applications.

Starting with version 3.0.0, Recaptcha.Verify.Net supports the following platforms:
- .NET 8
- .NET 9
- .NET 10

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
       "SecretKey": "<recaptcha secret key>",
       "AttributeOptions": {
         "ResponseTokenNameInHeader": "X-Recaptcha-Token",
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
Token extraction is performed by services implementing the `IRecaptchaTokenExtractor` interface.
The `RecaptchaAttribute` uses registered extractors sequentially until the first one successfully retrieves a token.

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
When using `services.AddRecaptcha()`, extractors are automatically registered based on provided configuration `RecaptchaOptions.AttributeOptions`:
- if `ResponseTokenNameInForm` is not empty then registers `FormTokenExtractor`;
- if `ResponseTokenNameInHeader` is not empty then registers `HeaderTokenExtractor`;
- if `ResponseTokenNameInQuery` is not empty then registers `QueryTokenExtractor`;
- if `GetResponseTokenFromActionArguments` delegate is not empty then registers `ActionArgumentsTokenExtractor`;
- if `GetResponseTokenFromExecutingContext` delegate is not empty then registers `ExecutingContextTokenExtractor`.

#### Important Notes
- Query Parameters: Avoid using `QueryTokenExtractor` as tokens in URLs may be logged or cached. Prefer headers or body.
- Request Body: Use `ActionArgumentsTokenExtractor` for body tokens, since the request stream is already read and models are populated by this stage.
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
  - Global action `RecaptchaOptions.Action`
- **Score Threshold**: Ensures the confidence score meets the required threshold
  - Provided in the `RecaptchaAttribute` constructor
  - Global threshold `RecaptchaOptions.ScoreThreshold`
  - Action-specific threshold specified in `RecaptchaOptions.ActionsScoreThresholds` dictionary

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
1. `RecaptchaOptions.Action` - global default action for all v3 validations

**For Score Thresholds**:
- `RecaptchaAttribute` constructor value
- `RecaptchaOptions.ActionsScoreThresholds[action]` - action-specific thresholds map
- `RecaptchaOptions.ScoreThreshold` - global default score threshold (By default, you can use a threshold of 0.5)

### Configuring Actions and Score Thresholds Example
```json
{
  "Recaptcha": {
    "Action": "default_action",
    "ScoreThreshold": 0.5,
    "ActionsScoreThresholds": {
      "login": 0.8,
      "comment": 0.3
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
To use a custom endpoint (e.g. a mirror or proxy), set the `BaseUrl` option in `RecaptchaOptions`:

### Via appsettings.json
```json
{
  "Recaptcha": {
    "SecretKey": "<recaptcha secret key>",
    "BaseUrl": "https://recaptcha-proxy.example.com/recaptcha/api",
    "AttributeOptions": {
      "ResponseTokenNameInHeader": "X-Recaptcha-Token"
    }
  }
}
```

### Via code configuration
```csharp
services.AddRecaptcha(o =>
{
    o.SecretKey = "<recaptcha secret key>";
    o.BaseUrl = "https://recaptcha-proxy.example.com/recaptcha/api";
});
```

When `BaseUrl` is not specified, the default Google endpoint is used.

## Response Customization
When reCAPTCHA verification fails or an exception occurs (will not catch exceptions in future versions), the library returns a `400 Bad Request` response by default.
Message specified in `RecaptchaOptions.VerificationFailedMessage` which default value is "Recaptcha verification failed".

|Delegate|Trigger|Status|
|---|---|---|
|`OnVerificationFailed`|When token verification or validation fails|Active|
|`OnRecaptchaServiceException`|When `RecaptchaServiceException` is thrown|Deprecated|
|`OnException`|When any unhandled exception occurs|Deprecated|
|`OnReturnBadRequest`|When any failure or exception occurs|Deprecated|

The `IActionResult` returned by these delegates is used as the HTTP response.
Delegates may also throw exceptions, which will not be caught by `RecaptchaAttribute`.

Deprecated delegates will be removed in future versions in favor of exception handling middleware.

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
`RecaptchaServiceProcessingException` | Base recaptcha exception for errors while processing token verification and result checking.
`EmptyCaptchaAnswerException` | This exception is thrown when captcha answer passed in function is empty.
`EmptyResponseException` | This exception is thrown when verification request response is empty. When thrown, it is wrapped in VerifyRequestException.
`VerifyRequestException` | This exception is thrown when verification request failed. Stores inner exception.
`UnknownErrorKeyException` | This exception is thrown when verification response error key is unknown.
`RecaptchaUnknownException` | This exception is thrown when an unexpected exception is catched during processing captcha.

## Examples
Examples could be found in library repository:
- [**Recaptcha.Verify.Net.ConsoleApp**](https://github.com/vese/Recaptcha.Verify.Net/blob/v3.0/examples/Recaptcha.Verify.Net.ConsoleApp/Program.cs) (.NET 10)
- [**Recaptcha.Verify.Net.AspNetCoreAngular**](https://github.com/vese/Recaptcha.Verify.Net/tree/release/v3.0/examples/Recaptcha.Verify.Net.AspNetCoreAngular) (ASP.NET + Angular)
