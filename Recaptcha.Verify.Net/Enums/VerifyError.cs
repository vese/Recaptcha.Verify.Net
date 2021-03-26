namespace Recaptcha.Verify.Net.Enums
{
    /// <summary>
    /// Verification errors.
    /// </summary>
    public enum VerifyError
    {
        /// <summary>
        /// The secret parameter is missing.
        /// </summary>
        MissingInputSecret,
        /// <summary>
        /// The secret parameter is invalid or malformed.
        /// </summary>
        InvalidInputSecret,
        /// <summary>
        /// The response parameter is missing.
        /// </summary>
        MissingInputResponse,
        /// <summary>
        /// The response parameter is invalid or malformed.
        /// </summary>
        InvalidInputResponse,
        /// <summary>
        /// The request is invalid or malformed.
        /// </summary>
        BadRequest,
        /// <summary>
        /// The response is no longer valid: either is too old or has been used previously.
        /// </summary>
        TimeoutOrDuplicate
    }
}
