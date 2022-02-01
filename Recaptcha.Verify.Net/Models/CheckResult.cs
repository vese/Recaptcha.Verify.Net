﻿namespace Recaptcha.Verify.Net.Models.Response
{
    /// <summary>
    /// Result of checking of verify response.
    /// </summary>
    public class CheckResult
    {
        /// <summary>
        /// Veryfy response.
        /// </summary>
        public VerifyResponse Response { get; set; }

        /// <summary>
        /// <c>True</c> if action matches specified value.
        /// </summary>
        public bool ActionMatches { get; set; }

        /// <summary>
        /// <c>True</c> if score satisfies specified threshold.
        /// For reCAPTCHA v2 value is <c>False</c>.
        /// </summary>
        public bool ScoreSatisfies { get; set; }

        /// <summary>
        /// <c>True</c> if successfully verified and satisfies specified requirements.
        /// </summary>
        public bool Success => Response.Success && ActionMatches && (!Response.Score.HasValue || ScoreSatisfies);
    }
}