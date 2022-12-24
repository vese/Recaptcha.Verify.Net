using System;
using System.Runtime.Serialization;

namespace Recaptcha.Verify.Net.Exceptions
{
    /// <summary>
    /// This exception is thrown when minimal score was not specified and request had score value.
    /// </summary>
    [Serializable]
    public class MinScoreNotSpecifiedException : RecaptchaServiceException
    {
        /// <summary>
        /// The action.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinScoreNotSpecifiedException"/> class.
        /// </summary>
        public MinScoreNotSpecifiedException(string action) : base($"Score threshold was not provided for action {action}.")
        {
            Action = action;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinScoreNotSpecifiedException"/> class with serialized data.
        /// </summary>
        protected MinScoreNotSpecifiedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Action = info.GetString(nameof(Action));
        }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Action), Action);
            base.GetObjectData(info, context);
        }
    }
}
