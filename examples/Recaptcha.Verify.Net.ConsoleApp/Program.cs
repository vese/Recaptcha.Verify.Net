using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.Exceptions;
using Recaptcha.Verify.Net.TokenVerification;
using Recaptcha.Verify.Net.VerificationResultValidation;
using System.Text.Json;

/// <summary>
/// Test secret key for reCAPTCHA v2.
/// https://developers.google.com/recaptcha/docs/faq#id-like-to-run-automated-tests-with-recaptcha.-what-should-i-do
/// </summary>
var ValidSecretKey = "6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe";
var InvalidSecretKey = "<invalid secret key>";

try
{
    var invalidServiceProvider = CreateServiceProvider(InvalidSecretKey);
    var recaptchaVerificationServiceWithInvalidKey = invalidServiceProvider.GetRequiredService<IRecaptchaVerificationService>();

    // Token verification with invalid secret key
    var failureResponse = await recaptchaVerificationServiceWithInvalidKey.VerifyAsync("<response token>");
    Console.WriteLine("Result of verification with invalid secret key:");
    Console.WriteLine(JsonSerializer.Serialize(failureResponse));

    var validServiceProvider = CreateServiceProvider(ValidSecretKey);
    var recaptchaVerificationService = validServiceProvider.GetRequiredService<IRecaptchaVerificationService>();
    var recaptchaValidationService = validServiceProvider.GetRequiredService<IRecaptchaVerificationResultValidationService>();

    // Token verification with valid secret key
    var successResponse = await recaptchaVerificationService.VerifyAsync("<response token>");
    Console.WriteLine("Result of verification with valid secret key:");
    Console.WriteLine(JsonSerializer.Serialize(successResponse));

    // Validation of successful verification result
    var checkResult = recaptchaValidationService.Validate(successResponse, "test");
    Console.WriteLine("Result of verification result validation:");
    Console.WriteLine(JsonSerializer.Serialize(checkResult));

    if (checkResult.Success)
    {
        // Handle successful validation
    }
    else if (!checkResult.ResponseSuccessful)
    {
        // Handle negative verification result
    }
    else if (!checkResult.ActionMatches)
    {
        // Handle action not matches for v3
    }
    else if (!checkResult.ScoreSatisfies)
    {
        // Handle score less than specified threshold for v3
    }
}
catch (RecaptchaServiceException e)
{
    // Handle exceptions in service
    Console.WriteLine(e.Message);
}

Console.ReadLine();

static ServiceProvider CreateServiceProvider(string secretKey) =>
    new ServiceCollection()
        .AddLogging(builder =>
        {
            builder.AddDebug();
            builder.AddConsole();
        })
        .AddRecaptcha(o =>
        {
            o.Verification.SecretKey = secretKey;
            o.Validation.ScoreThreshold = 0.5f;
        })
        .BuildServiceProvider();
