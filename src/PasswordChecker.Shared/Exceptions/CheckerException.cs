namespace PasswordChecker.Shared.Exceptions
{
    public abstract class CheckerException : Exception
    {
        #region Properties

        /// <summary>
        ///     Exception code
        /// </summary>
        public ExceptionCode Code { get; set; }

        #endregion Properties

        #region Constructor

        protected CheckerException(ExceptionCode code)
        {
            Code = code;
        }

        protected CheckerException(ExceptionCode code, string message) : base(message)
        {
            Code = code;
        }

        protected CheckerException(ExceptionCode code, string message, Exception innerException) : base(message, innerException)
        {
            Code = code;
        }

        #endregion Constructor
    }
}
