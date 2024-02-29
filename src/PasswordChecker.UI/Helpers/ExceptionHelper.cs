using System;
using PasswordChecker.Resources.Language;
using PasswordChecker.Shared.Exceptions;

namespace PasswordChecker.UI.Helpers
{
    public static class ExceptionHelper
    {
        /// <summary>
        ///     Returns the text of an exception
        /// </summary>
        public static string GetExceptionText(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            if (exception is CheckerException checkerEx)
            {
                var exceptionMessage = ExceptionResource.ResourceManager.GetString(checkerEx.Code.ToString());
                if (string.IsNullOrWhiteSpace(exceptionMessage))
                {
                    exceptionMessage = $"ERROR: {checkerEx.GetType().Name}; {checkerEx.Code}";
                }

                return exceptionMessage;
            }

            return exception.ToString();
        }
    }
}
