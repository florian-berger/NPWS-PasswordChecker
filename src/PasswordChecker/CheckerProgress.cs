using System.Diagnostics;
using PasswordChecker.Data;
using Prism.Mvvm;

namespace PasswordChecker
{
    /// <summary>
    ///     Progress of the current check
    /// </summary>
    public class CheckerProgress : BindableBase
    {
        #region Properties

        /// <summary>
        ///     Time when the check was started
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        ///     Step that is currently executing
        /// </summary>
        public CheckerStep Step
        {
            get => _step;
            set
            {
                if (SetProperty(ref _step, value))
                {
                    RaisePropertyChanged(nameof(StepInt));
                }
            }
        } private CheckerStep _step = CheckerStep.NotStarted;

        /// <summary>
        ///     Integer value representing the current step
        /// </summary>
        public int StepInt
        {
            get
            {
                Trace.WriteLine(Step);
                return (int)Step;
            }
        } 

        public bool IsFinished
        {
            get => _isFinished;
            set => SetProperty(ref _isFinished, value);
        } private bool _isFinished;

        public ReportData? Report
        {
            get => _report;
            set => SetProperty(ref _report, value);
        } private ReportData? _report;

        #endregion Properties
    }
}
