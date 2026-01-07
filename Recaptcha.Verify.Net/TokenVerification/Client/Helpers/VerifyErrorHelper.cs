namespace Recaptcha.Verify.Net.TokenVerification.Client.Helpers;

internal static class VerifyErrorHelper
{
    internal static readonly Dictionary<string, VerifyError> VerifyErrorsDictionary = new()
    {
        { "missing-input-secret", VerifyError.MissingInputSecret },
        { "invalid-input-secret", VerifyError.InvalidInputSecret },
        { "missing-input-response", VerifyError.MissingInputResponse },
        { "invalid-input-response", VerifyError.InvalidInputResponse },
        { "bad-request", VerifyError.BadRequest },
        { "timeout-or-duplicate", VerifyError.TimeoutOrDuplicate }
    };

    internal static IReadOnlyCollection<VerifyError>? GetVerifyErrors(IEnumerable<string>? errors) =>
        errors?.Select(error =>
        {
            if (VerifyErrorsDictionary.TryGetValue(error, out var verifyError))
            {
                return verifyError;
            }

            throw new UnknownErrorKeyException(error);
        }).ToList();
}
