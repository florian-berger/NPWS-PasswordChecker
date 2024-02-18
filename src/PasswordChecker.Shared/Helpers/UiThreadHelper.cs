namespace PasswordChecker.Shared.Helpers
{
    public class UiThreadHelper
    {
        #region Private variables

        private static int _uiThreadId;
        private static TaskFactory? _taskFactory;

        #endregion Private variables

        public UiThreadHelper()
        {
            _uiThreadId = Environment.CurrentManagedThreadId;

            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _taskFactory = new TaskFactory(scheduler);
        }

        #region Public methods

        public static void RunOnUiThread(Action action)
        {
            if (IsOnUi())
            {
                action?.Invoke();
            }
            else
            {
                _taskFactory.StartNew(action);
            }
        }

        public static async Task RunAsyncOnUiThread(Func<Task> asyncAction)
        {
            if (IsOnUi())
            {
                await asyncAction();
            }
            else
            {
                await _taskFactory.StartNew(asyncAction).Unwrap();
            }
        }

        #endregion Public methods

        #region Private methods

        private static bool IsOnUi()
        {
            return Environment.CurrentManagedThreadId == _uiThreadId;
        }

        #endregion Private methods
    }
}
