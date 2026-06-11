using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Recaptcha.Verify.Net.Exceptions;
using Recaptcha.Verify.Net.Exceptions.Configuration;
using Recaptcha.Verify.Net.Exceptions.Processing;

namespace Recaptcha.Verify.Net.AspNetCoreAngular.Server;

public class RecaptchaExceptionHandler(IHostEnvironment environment, ILogger<RecaptchaExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not RecaptchaServiceException recaptchaException)
        {
            return false;
        }

        logger.LogError(recaptchaException, "reCAPTCHA service exception occurred");

        var statusCode = recaptchaException switch
        {
            RecaptchaServiceConfigurationException => StatusCodes.Status500InternalServerError,
            RecaptchaServiceProcessingException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = "reCAPTCHA verification failed",
            Detail = environment.IsDevelopment() ? recaptchaException.ToString() : null
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
