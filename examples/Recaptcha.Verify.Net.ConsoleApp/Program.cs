using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Recaptcha.Verify.Net.Configuration;
using Recaptcha.Verify.Net.Exceptions;
using System;
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
                var recaptchaServiceWithInvalidKey = CreateService(InvalidSecretKey);

                var failureResponse = await recaptchaServiceWithInvalidKey.VerifyAsync("<response token>");
                Console.WriteLine(JsonConvert.SerializeObject(failureResponse));
                Console.WriteLine();

                var recaptchaService = CreateService(ValidSecretKey);

                var successResponse = await recaptchaService.VerifyAsync("<response token>");
                Console.WriteLine(JsonConvert.SerializeObject(successResponse));
                Console.WriteLine();

                // Verifies response token and checks action and score for v3
                var checkResult = await recaptchaService.VerifyAndCheckAsync("<response token>", "test");
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
                Console.WriteLine(e.Message);
            }
        }

        static IRecaptchaService CreateService(string secretKey)
        {
            var serviceProvider = new ServiceCollection()
                .AddRecaptcha(builder =>
                {
                    builder.Configure(new RecaptchaOptions(), o =>
                    {
                        o.SecretKey = secretKey;
                        o.ScoreThreshold = 0.5f;
                    });
                })
                .BuildServiceProvider();

            var recaptchaService = serviceProvider.GetRequiredService<IRecaptchaService>();
            return recaptchaService;
        }
    }
}
