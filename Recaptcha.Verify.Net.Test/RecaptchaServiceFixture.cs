using Microsoft.Extensions.Options;
using Moq;
using Recaptcha.Verify.Net.Enums;
using Recaptcha.Verify.Net.Helpers;
using Recaptcha.Verify.Net.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recaptcha.Verify.Net.Test;

public record ResponseTokenFixture(string Token, string? Action, string? Hostname, string? ApkPackageName, float? Score, List<string>? Errors)
{
    public List<VerifyError> ErrorsItems => EnumHelper.GetVerifyErrors(Errors);
}

public static class RecaptchaServiceFixture
{
    /// <summary>
    /// Test secret key for reCAPTCHA v2.
    /// https://developers.google.com/recaptcha/docs/faq#id-like-to-run-automated-tests-with-recaptcha.-what-should-i-do
    /// </summary>
    public const string ValidSecretKey = "6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe";
    public const string InvalidSecretKey = "<invalid secret key>";
    public const string Token = "token";
    public const string Action = "action";
    public const float Score = 0.5f;
    public const float ScoreUnsuccessful = 2.0f;
    public const string Hostname = "hostname";
    public const string ApkPackageName = "action";

    public static Dictionary<string, float> ScoreMap => new()
    {
        { ValidTokenFixture10.Action!, 0.9f },
        { ValidTokenFixture05.Action!, 0.5f },
        { ValidTokenFixture02.Action!, 0.3f },
    };

    public static List<string> SecretKeys => new() { InvalidSecretKey, ValidSecretKey };

    public static readonly List<string> ErrorsKeys = EnumHelper.VerifyErrorsDictionary.Keys.ToList();
    public static readonly string UnknownErrorKey = "unknown-error-key";

    public static readonly ResponseTokenFixture InvalidTokenFixtureUnknownErrorKey =
        new("token1", null, null, null, null, new() { UnknownErrorKey });
    public static readonly ResponseTokenFixture InvalidTokenFixtureOneKey =
        new("token2", null, null, null, null, new() { ErrorsKeys.First() });
    public static readonly ResponseTokenFixture InvalidTokenFixtureAllKeys =
        new("token3", null, null, null, null, ErrorsKeys);

    public static readonly ResponseTokenFixture ValidTokenFixture10 =
        new("token4", "action4", Hostname, null, 1.0f, null);
    public static readonly ResponseTokenFixture ValidTokenFixture05 =
        new("token5", "action5", Hostname, null, 0.5f, null);
    public static readonly ResponseTokenFixture ValidTokenFixture02 =
        new("token6", "action6", null, ApkPackageName, 0.2f, null);

    public static List<ResponseTokenFixture> AllTokenFixtures => new()
    {
        InvalidTokenFixtureUnknownErrorKey,
        InvalidTokenFixtureOneKey,
        InvalidTokenFixtureAllKeys,
        ValidTokenFixture10,
        ValidTokenFixture05,
        ValidTokenFixture02,
    };

    public static List<ResponseTokenFixture> ValidTokenFixtures => new()
    {
        ValidTokenFixture10,
        ValidTokenFixture05,
        ValidTokenFixture02,
    };

    public static List<ResponseTokenFixture> InvalidTokenFixtures => new()
    {
        InvalidTokenFixtureOneKey,
        InvalidTokenFixtureAllKeys,
    };

    public static IRecaptchaService CreateService(IOptions<RecaptchaOptions> options, bool v3 = true)
    {
        var recaptchaClientMock = new Mock<IRecaptchaClient>();

        recaptchaClientMock
            .Setup(x => x.VerifyAsync(
                It.Is<VerifyRequest>(req => req.Secret == InvalidSecretKey),
                default))
            .Returns(Task.FromResult(new VerifyResponse() { Success = false, Score = null }));

        foreach (var tokenFixture in AllTokenFixtures)
        {
            recaptchaClientMock
                .Setup(x => x.VerifyAsync(
                    It.Is<VerifyRequest>(req => req.Secret == ValidSecretKey && req.Response == tokenFixture.Token),
                    default))
                .Returns(Task.FromResult(new VerifyResponse()
                {
                    Success = tokenFixture.Score != null,
                    Score = v3 ? tokenFixture.Score : null,
                    Action = v3 ? tokenFixture.Action : null,
                    ChallengeTs = DateTime.Now,
                    Hostname = tokenFixture.Hostname,
                    ApkPackageName = tokenFixture.ApkPackageName,
                    ErrorCodes = tokenFixture.Errors,
                }));
        }

        return new RecaptchaService(options, recaptchaClientMock.Object);
    }
}
