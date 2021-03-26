using Recaptcha.Verify.Net.Enums;
using Recaptcha.Verify.Net.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace Recaptcha.Verify.Net.Helpers
{
    internal static class EnumHelper
    {
        private static readonly Dictionary<string, VerifyError> _verifyErrorsDictionary = new Dictionary<string, VerifyError>()
        {
            { "missing-input-secret", VerifyError.MissingInputSecret },
            { "invalid-input-secret", VerifyError.InvalidInputSecret },
            { "missing-input-response", VerifyError.MissingInputResponse },
            { "invalid-input-response", VerifyError.InvalidInputResponse },
            { "bad-request", VerifyError.BadRequest },
            { "timeout-or-duplicate", VerifyError.TimeoutOrDuplicate }
        };

        internal static List<VerifyError> GetVerifyErrors(List<string> errors) =>
            errors?.Select(error =>
            {
                if (_verifyErrorsDictionary.TryGetValue(error, out var verifyError))
                {
                    return verifyError;
                }

                throw new RecaptchaServiceException("Unknown exception key");
            }).ToList();
    }
}
