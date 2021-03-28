using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Recaptcha.Verify.Net.Exceptions;
using Recaptcha.Verify.Net.Models;
using Recaptcha.Verify.Net.Models.Request;
using Recaptcha.Verify.Net.Models.Response;
using Refit;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Recaptcha.Verify.Net.ConsoleApp
{
    class Program
    {
        /// <summary>
        /// Test secret key for reCAPTCHA v2.
        /// https://developers.google.com/recaptcha/docs/faq#id-like-to-run-automated-tests-with-recaptcha.-what-should-i-do
        /// </summary>
        private const string ValidSecretKey = "6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe";
        private const string InvalidSecretKey = "<invalid secret key>";

        static async Task Main(string[] args)
        {
            try
            {
                var successResponse = await Verify(ValidSecretKey, "<response token>");
                Console.WriteLine(JsonConvert.SerializeObject(successResponse));

                var failureResponse = await Verify(InvalidSecretKey, "<response token>");
                Console.WriteLine(JsonConvert.SerializeObject(failureResponse));
            }
            catch (RecaptchaServiceException e)
            {
                // Handle exceptions in service
            }
        }

        private static async Task<VerifyResponse> Verify(string secretKey, string responseToken)
        {
            var options = Options.Create(new RecaptchaOptions()
            {
                SecretKey = secretKey
            });

            var recaptchaClient = RestService.For<IRecaptchaClient>(
                new HttpClient()
                {
                    BaseAddress = new Uri("https://www.google.com/recaptcha/api")
                },
                new RefitSettings(new NewtonsoftJsonContentSerializer()));

            var recaptchaService = new RecaptchaService(options, recaptchaClient);

            var response = await recaptchaService.VerifyAsync(responseToken);

            return response;
        }
    }
}
