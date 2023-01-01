using Recaptcha.Verify.Net.Attribute;
using System.Collections.Generic;

namespace Recaptcha.Verify.Net.Test.Tests;

public class BaseRecaptchaAttributeTest
{
    private static readonly object[] AttributeWithoutScore = new object[] { new RecaptchaAttribute(RecaptchaServiceFixture.Action) };

    private static readonly object[] AttributeWithScore = new object[] { new RecaptchaAttribute(RecaptchaServiceFixture.Action, RecaptchaServiceFixture.Score) };

    public static IEnumerable<object[]> AttributeDataWithoutScore() => new List<object[]> { AttributeWithoutScore };

    public static IEnumerable<object[]> AttributeDataWithScore() => new List<object[]> { AttributeWithScore };

    public static IEnumerable<object[]> AttributeData() => new List<object[]>
    {
        AttributeWithoutScore,
        AttributeWithScore,
    };
}
