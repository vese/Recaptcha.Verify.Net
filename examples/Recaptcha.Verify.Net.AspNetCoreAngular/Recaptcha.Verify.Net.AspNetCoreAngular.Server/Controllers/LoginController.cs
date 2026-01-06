using Microsoft.AspNetCore.Mvc;
using Recaptcha.Verify.Net.AspNetCoreAngular.Server.Models;
using Recaptcha.Verify.Net.Attribute;
using Recaptcha.Verify.Net.TokenVerification;
using Recaptcha.Verify.Net.VerificationResultValidation;

namespace Recaptcha.Verify.Net.AspNetCoreAngular.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController(
    ILogger<LoginController> logger,
    IRecaptchaVerificationService recaptchaVerificationService,
    IRecaptchaVerificationResultValidationService recaptchaValidationService) : Controller
{
    [Recaptcha("login")]
    [HttpPost("Login")]
    public IActionResult Login_RecaptchaAttribute([FromForm] Credentials credentials, CancellationToken cancellationToken)
    {
        string[] t = ["asd", "sdsds"];
        // Process login
        logger.LogError("Recaptcha error: {errorCodes}", t);

        return Ok();
    }

    [Recaptcha("login")]
    [HttpPost("LoginInBody")]
    public IActionResult LoginInBody([FromBody] Credentials credentials, CancellationToken cancellationToken)
    {
        string[] t = ["asd", "sdsds"];
        // Process login
        logger.LogError("Recaptcha error: {errorCodes}", t);

        return Ok();
    }

    [HttpPost("Login_ServicesExample")]
    public async Task<IActionResult> Login_ServicesExample([FromBody] Credentials credentials, CancellationToken cancellationToken)
    {
        var verificationResult = await recaptchaVerificationService.VerifyAsync(
            credentials.RecaptchaToken,
            HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
            cancellationToken);

        var validationResult = recaptchaValidationService.Validate(verificationResult, "login");

        if (!validationResult.Success)
        {
            if (!validationResult.ResponseSuccessful)
            {
                // Handle unsuccessful verification response
                logger.LogError("Recaptcha error: {errorCodes}", verificationResult.ErrorCodes);
            }

            if (!validationResult.ScoreSatisfies)
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
