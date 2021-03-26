using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Recaptcha.Verify.Net.AspNetCoreAngular.Models;
using Recaptcha.Verify.Net.Models.Request;
using System.Threading;
using System.Threading.Tasks;

namespace Recaptcha.Verify.Net.AspNetCoreAngular.Controllers
{
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
}
