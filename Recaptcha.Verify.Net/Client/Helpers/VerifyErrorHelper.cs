using Recaptcha.Verify.Net.Client.Models.Response;
using Recaptcha.Verify.Net.Exceptions.Processing;

namespace Recaptcha.Verify.Net.Client.Helpers;

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

    internal static List<VerifyError>? GetVerifyErrors(List<string>? errors) =>
        errors?.Select(error =>
        {
            if (VerifyErrorsDictionary.TryGetValue(error, out var verifyError))
            {
                return verifyError;
            }

            throw new UnknownErrorKeyException(error);
        }).ToList();
}
