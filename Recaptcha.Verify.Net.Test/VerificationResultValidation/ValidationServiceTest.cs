using Recaptcha.Verify.Net.Exceptions.Configuration;
using Xunit;

namespace Recaptcha.Verify.Net.Test.VerificationResultValidation;

public class ValidationServiceTest
{
    public static TheoryData<string?> EmptyStrings => new(null, string.Empty, "   ");

    public static TheoryData<VerifyResponseTestData> SuccessfulVerificationResults =>
        [.. VerificationResultValidationFixture.SuccessfulVerificationResults.Select(VerifyResponseTestData.Create)];

    [Theory]
    [MemberData(nameof(EmptyStrings))]
    public void Validate_v3_EmptyAction_Throws(string? action)
    {
        var verificationResult = VerificationResultValidationFixture.SuccessfulVerificationResults.First();

        var validationService = ValidationServiceFixture.Create(null, null, null);

        Assert.Throws<EmptyActionException>(() => validationService.Validate(verificationResult, action));

        validationService = ValidationServiceFixture.Create(action, null, null);

        Assert.Throws<EmptyActionException>(() => validationService.Validate(verificationResult));
    }

    [Fact]
    public void Validate_v3_ScoreNotSpecified_Throws()
    {
        var verificationResult = VerificationResultValidationFixture.SuccessfulVerificationResults.First();

        var validationService = ValidationServiceFixture.Create(null, null, null);

        Assert.Throws<MinScoreNotSpecifiedException>(() => validationService.Validate(verificationResult, VerificationResultValidationFixture.Action));

        validationService = ValidationServiceFixture.Create(VerificationResultValidationFixture.Action, null,
            VerificationResultValidationFixture.ActionsScoresEmpty);

        Assert.Throws<MinScoreNotSpecifiedException>(() => validationService.Validate(verificationResult));
    }

    [Fact]
    public void Validate_v3_UnsuccessfulVerification()
    {
        var verificationResult = VerificationResultValidationFixture.UnsuccessfulVerificationResult;

        var validationService = ValidationServiceFixture.Create(null, null, null);

        var validationResult = validationService.Validate(verificationResult);

        Assert.False(validationResult.IsV3);
        Assert.False(validationResult.ResponseSuccessful);
        Assert.False(validationResult.ActionMatches);
        Assert.False(validationResult.ScoreSatisfies);
        Assert.False(validationResult.Success);
    }

    [Fact]
    public void Validate_v2_SuccessfulVerification()
    {
        var verificationResult = VerificationResultValidationFixture.V2SuccessfulVerificationResult;

        var validationService = ValidationServiceFixture.Create(null, null, null);

        var validationResult = validationService.Validate(verificationResult);

        Assert.False(validationResult.IsV3);
        Assert.True(validationResult.ResponseSuccessful);
        Assert.False(validationResult.ActionMatches);
        Assert.False(validationResult.ScoreSatisfies);
        Assert.True(validationResult.Success);
    }

    [Theory]
    [MemberData(nameof(SuccessfulVerificationResults))]
    public void Validate_v3_SuccessfulVerification_WithScoreThreshold(VerifyResponseTestData verificationResult)
    {
        var validationService = ValidationServiceFixture.Create(
            VerificationResultValidationFixture.Action, VerificationResultValidationFixture.Score, null);

        var validationResult = validationService.Validate(verificationResult);

        var scoreSatisfies = verificationResult.Score >= VerificationResultValidationFixture.Score;

        Assert.True(validationResult.IsV3);
        Assert.True(validationResult.ResponseSuccessful);
        Assert.True(validationResult.ActionMatches);
        Assert.Equal(scoreSatisfies, validationResult.ScoreSatisfies);
        Assert.Equal(scoreSatisfies, validationResult.Success);
    }

    [Theory]
    [MemberData(nameof(SuccessfulVerificationResults))]
    public void Validate_v3_SuccessfulVerification_WithActionsScoreThresholds(VerifyResponseTestData verificationResult)
    {
        var validationService = ValidationServiceFixture.Create(VerificationResultValidationFixture.Action, null,
            VerificationResultValidationFixture.ActionsScores);

        var validationResult = validationService.Validate(verificationResult);

        var scoreSatisfies = verificationResult.Score >= VerificationResultValidationFixture.Score;

        Assert.True(validationResult.IsV3);
        Assert.True(validationResult.ResponseSuccessful);
        Assert.True(validationResult.ActionMatches);
        Assert.Equal(scoreSatisfies, validationResult.ScoreSatisfies);
        Assert.Equal(scoreSatisfies, validationResult.Success);
    }

    [Theory]
    [MemberData(nameof(SuccessfulVerificationResults))]
    public void Validate_v3_SuccessfulVerification_WithScoreThresholdDirectly(VerifyResponseTestData verificationResult)
    {
        var validationService = ValidationServiceFixture.Create(null, null, null);

        var validationResult = validationService.Validate(verificationResult,
            VerificationResultValidationFixture.Action, VerificationResultValidationFixture.Score);

        var scoreSatisfies = verificationResult.Score >= VerificationResultValidationFixture.Score;

        Assert.True(validationResult.IsV3);
        Assert.True(validationResult.ResponseSuccessful);
        Assert.True(validationResult.ActionMatches);
        Assert.Equal(scoreSatisfies, validationResult.ScoreSatisfies);
        Assert.Equal(scoreSatisfies, validationResult.Success);
    }

    [Theory]
    [MemberData(nameof(SuccessfulVerificationResults))]
    public void Validate_v3_SuccessfulVerification_WithScoreThresholdDirectly_OverridesFromOptions(VerifyResponseTestData verificationResult)
    {
        var validationService = ValidationServiceFixture.Create(VerificationResultValidationFixture.Action2,
            VerificationResultValidationFixture.Score2, VerificationResultValidationFixture.ActionsScores2);

        var validationResult = validationService.Validate(verificationResult,
            VerificationResultValidationFixture.Action, VerificationResultValidationFixture.Score);

        var scoreSatisfies = verificationResult.Score >= VerificationResultValidationFixture.Score;

        Assert.True(validationResult.IsV3);
        Assert.True(validationResult.ResponseSuccessful);
        Assert.True(validationResult.ActionMatches);
        Assert.Equal(scoreSatisfies, validationResult.ScoreSatisfies);
        Assert.Equal(scoreSatisfies, validationResult.Success);
    }
}
