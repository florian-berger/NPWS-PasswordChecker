namespace PasswordChecker.Data
{
    public class ReportData(
        int totalCountOfPasswordFields,
        int skippedCountOfPasswordFields,
        List<LeakedPassword> breachedPasswords,
        QualityStore quality,
        List<string>[] duplicates,
        Dictionary<string, Exception> errors,
        ConnectionInfo connectionInfo
    )
    {
        #region Properties

        /// <summary>
        ///     Information when the check was started
        /// </summary>
        public DateTime CheckStart { get; set; }

        /// <summary>
        ///     Information when the check was finished
        /// </summary>
        public DateTime CheckEnd { get; set; }

        /// <summary>
        ///     Information how long the check was running
        /// </summary>
        public TimeSpan Duration => CheckEnd - CheckStart;

        public int TotalCountOfPasswordFields { get; init; } = totalCountOfPasswordFields;

        public int SkippedCountOfPasswordFields { get; init; } = skippedCountOfPasswordFields;

        public List<LeakedPassword> BreachedPasswords { get; init; } = breachedPasswords;

        public QualityStore Quality { get; init; } = quality;

        public List<string>[] Duplicates { get; init; } = duplicates;

        public int DuplicatesCount => Duplicates.Sum(d => d.Count);

        public Dictionary<string, Exception> Errors { get; init; } = errors;

        /// <summary>
        ///     Data that was used for connection
        /// </summary>
        public ConnectionInfo ConnectionData { get; init; } = connectionInfo;

        #endregion Properties
    }
}
