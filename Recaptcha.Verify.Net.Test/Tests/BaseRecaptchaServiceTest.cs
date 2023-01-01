using Microsoft.Extensions.Options;
using Recaptcha.Verify.Net.Configuration;
using System.Collections.Generic;

namespace Recaptcha.Verify.Net.Test.Tests;

public abstract class BaseRecaptchaServiceTest
{
    public static readonly string? NullString = null;
    public static readonly string EmptyString = string.Empty;
    public static readonly string WhiteSpaceString = "   ";

    public static List<string?> EmptyStrings => new() { NullString, EmptyString, WhiteSpaceString };

    private bool _v3 { get; init; }

    public BaseRecaptchaServiceTest(bool v3 = true)
    {
        _v3 = v3;
    }

    public static IEnumerable<object?[]> NoSecretKeyData()
    {
        foreach (var emptyKey in EmptyStrings)
        {
            yield return new object?[] { emptyKey, NullString };
            yield return new object?[] { emptyKey, RecaptchaServiceFixture.ValidTokenFixture10.Token };
        }
    }

    public static IEnumerable<object?[]> NoResponseTokenData()
    {
        foreach (var secretKey in RecaptchaServiceFixture.SecretKeys)
        {
            foreach (var token in EmptyStrings)
            {
                yield return new object?[] { secretKey, token };
            }
        }
    }

    public static IEnumerable<object[]> InvalidSecretKeyData()
    {
        return new List<object[]>
        {
            new object[] { RecaptchaServiceFixture.InvalidSecretKey, RecaptchaServiceFixture.ValidTokenFixture10 },
        };
    }

    public static IEnumerable<object[]> ValidItemsData()
    {
        foreach (var tokenFixture in RecaptchaServiceFixture.ValidTokenFixtures)
        {
            yield return new object[] { RecaptchaServiceFixture.ValidSecretKey, tokenFixture };
        }
    }

    public static IEnumerable<object[]> ValidItemData()
    {
        return new List<object[]>
        {
            new object[] { RecaptchaServiceFixture.ValidSecretKey, RecaptchaServiceFixture.ValidTokenFixture05 },
        };
    }

    public static IEnumerable<object[]> InvalidItemsData()
    {
        foreach (var tokenFixture in RecaptchaServiceFixture.InvalidTokenFixtures)
        {
            yield return new object[] { RecaptchaServiceFixture.ValidSecretKey, tokenFixture };
        }
    }

    public static IEnumerable<object[]> InvalidItemsWithUnknownErrorKeyData()
    {
        return new List<object[]>
        {
         new object[] { RecaptchaServiceFixture.ValidSecretKey, RecaptchaServiceFixture.InvalidTokenFixtureUnknownErrorKey },
        };
    }

    protected IRecaptchaService CreateService(
        string? secretKey = null, float? score = null, Dictionary<string, float>? scoreMap = null, string? action = null)
    {
        var options = Options.Create(CreateOptions(secretKey, score, scoreMap, action));
        return RecaptchaServiceFixture.CreateServiceWithClient(options, _v3);
    }

    protected RecaptchaOptions CreateOptions(
        string? secretKey = null, float? score = null, Dictionary<string, float>? scoreMap = null, string? action = null)
    {
        return new RecaptchaOptions()
        {
            SecretKey = secretKey,
            Action = action,
            ScoreThreshold = score,
            ActionsScoreThresholds = scoreMap,
        };
    }
}