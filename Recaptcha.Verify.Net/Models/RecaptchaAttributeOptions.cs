using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Recaptcha.Verify.Net.Exceptions;
using System;
using System.Collections.Generic;

namespace Recaptcha.Verify.Net.Models
{
    /// <summary>
    /// <see cref="RecaptchaAttribute"/> options.
    /// </summary>
    public class RecaptchaAttributeOptions
    {
        /// <summary>
        /// <c>True</c> if need to use cancellation token for validation and checking.
        /// <para>Default value is <c>True</c>.</para>
        /// </summary>
        public bool UseCancellationToken { get; set; } = true;

        /// <summary>
        /// Name of reCAPTCHA response token param in request header.
        /// </summary>
        public string ResponseTokenNameInHeader { get; set; }

        /// <summary>
        /// Name of reCAPTCHA response token param in request query.
        /// </summary>
        public string ResponseTokenNameInQuery { get; set; }

        /// <summary>
        /// Name of reCAPTCHA response token param in request form data.
        /// <para>Default value is "g-recaptcha-response".</para>
        /// </summary>
        public string ResponseTokenNameInForm { get; set; } = "g-recaptcha-response";

        /// <summary>
        /// Delegate for getting reCAPTCHA response token from action arguments.
        /// Actions argumets are mapped arguments of controller method.
        /// </summary>
        public Func<IDictionary<string, object>, string> GetResponseTokenFromActionArguments { get; set; }

        /// <summary>
        /// Delegate for getting reCAPTCHA response token from executing context.
        /// </summary>
        public Func<ActionExecutingContext, string> GetResponseTokenFromExecutingContext { get; set; }

        /// <summary>
        /// Delegate for handling failed verification of reCAPTCHA response token.
        /// <para>Returned <see cref="IActionResult"/> will be returned for whole request.</para>
        /// <para>Any exception could be thrown and will be propagated further.</para>
        /// </summary>
        public Func<ActionExecutingContext, string, CheckResult, IActionResult> OnVerificationFailed { get; set; }

        /// <summary>
        /// Delegate for handling thrown <see cref="RecaptchaServiceException"/> during verification of reCAPTCHA response token.
        /// <para>Returned <see cref="IActionResult"/> will be returned for whole request.</para>
        /// <para>Any exception could be thrown and will be propagated further.</para>
        /// </summary>
        public Func<ActionExecutingContext, string, CheckResult, RecaptchaServiceException, IActionResult> OnRecaptchaServiceException { get; set; }

        /// <summary>
        /// Delegate for handling thrown <see cref="Exception"/> during verification of reCAPTCHA response token.
        /// <para>Returned <see cref="IActionResult"/> will be returned for whole request.</para>
        /// <para>Any exception could be thrown and will be propagated further.</para>
        /// </summary>
        public Func<ActionExecutingContext, string, CheckResult, Exception, IActionResult> OnException { get; set; }

        /// <summary>
        /// Delegate for handling any bad result of verification.
        /// <para>Returned <see cref="IActionResult"/> will be returned for whole request.</para>
        /// <para>Any exception could be thrown and will be propagated further.</para>
        /// <para>Fires after <see cref="OnVerificationFailed"/>, <see cref="OnRecaptchaServiceException"/> and <see cref="OnException"/>.</para>
        /// </summary>
        public Func<ActionExecutingContext, string, CheckResult, RecaptchaServiceException, Exception, IActionResult> OnReturnBadRequest { get; set; }
    }
}
