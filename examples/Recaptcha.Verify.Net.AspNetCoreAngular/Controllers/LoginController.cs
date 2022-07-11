using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Recaptcha.Verify.Net.AspNetCoreAngular.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Recaptcha.Verify.Net.AspNetCoreAngular.Controllers
{
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

        [Recaptcha("login")]
        [HttpPost("Login_RecaptchaAttribute")]
        public async Task<IActionResult> Login_RecaptchaAttribute([FromForm] Credentials credentials, CancellationToken cancellationToken)
        {
            // Process login

            return Ok();
        }
    }
}
