namespace Recaptcha.Verify.Net.Exceptions;

internal static class ExceptionExtensions
{
    public static T WithLog<T>(this T e, Action<T> logAction) where T : RecaptchaServiceException
    {
        logAction.Invoke(e);
        return e;
    }
}
