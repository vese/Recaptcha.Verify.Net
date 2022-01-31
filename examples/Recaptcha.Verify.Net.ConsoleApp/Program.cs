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
                Console.WriteLine();

                var failureResponse = await Verify(InvalidSecretKey, "<response token>");
                Console.WriteLine(JsonConvert.SerializeObject(failureResponse));
                Console.WriteLine();

                // Verifies response token and checks action and score (score for v3)
                var checkResult = await VerifyAndCheck(ValidSecretKey, "<response token>", "test", 0.5f);
                Console.WriteLine(JsonConvert.SerializeObject(checkResult));
                if (checkResult.Success)
                {
                    // Handle successfully verified
                }
                else if (!checkResult.ScoreSatisfies)
                {
                    // Handle score less than specified threshold for v3
                }
                else
                {
                    // Handle negative response
                }
            }
            catch (RecaptchaServiceException e)
            {
                // Handle exceptions in service
            }
        }

        private static Task<VerifyResponse> Verify(string secretKey, string responseToken)
        {
            var recaptchaService = CreateService(secretKey);

            return recaptchaService.VerifyAsync(responseToken);
        }

        private static Task<CheckResult> VerifyAndCheck(
            string secretKey, string responseToken, string action, float score)
        {
            var recaptchaService = CreateService(secretKey, score);

            return recaptchaService.VerifyAndCheckAsync(responseToken, action);
        }

        private static IRecaptchaService CreateService(string secretKey, float? score = null)
        {
            var options = Options.Create(new RecaptchaOptions()
            {
                SecretKey = secretKey,
                ScoreThreshold = score
            });

            var recaptchaClient = RestService.For<IRecaptchaClient>(
                new HttpClient()
                {
                    BaseAddress = new Uri("https://www.google.com/recaptcha/api")
                },
                new RefitSettings(new NewtonsoftJsonContentSerializer()));

            return new RecaptchaService(options, recaptchaClient);
        }
    }
}
