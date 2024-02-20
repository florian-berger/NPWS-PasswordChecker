using System.Text.RegularExpressions;
using PasswordChecker.Data;
using PasswordChecker.Shared.Helpers;
using PsrApi;
using PsrApi.Data;
using PsrApi.Data.Enums;
using PasswordChecker.Helpers;
using PasswordChecker.ServiceConnections;

namespace PasswordChecker
{
    /// <summary>
    ///     Class that executes the check
    /// </summary>
    public class Checker : IDisposable
    {
        #region Constants

        /// <summary>
        ///     Number of passwords that will be loaded at the same time via Password Secure API
        /// </summary>
        private const int PasswordsCountPerPage = 100;

        #endregion Constants

        #region Private variables

        private readonly PsrApi.PsrApi _api;
        private readonly CheckerProgress _progress;
        private List<string> _ignoredFieldNames;
        private readonly ConnectionInfo _connectionInfo;
        private readonly CancellationToken _cancellationToken;

        private readonly Dictionary<string, List<string>> _knownPasswords;
        private readonly Dictionary<string, int> _breachedPasswords;
        private readonly Dictionary<string, Exception> _checkExceptions;

        private int _fieldsCount;
        private int _skippedFieldsCount;

        private readonly (List<string?> WeakPasswords, List<string?> GoodPasswords, List<string?> StrongPasswords)
            _qualityStore = ([], [], []);

        #endregion Private variables

        #region Constructor

        public Checker(PsrApi.PsrApi api, CheckerProgress progress, List<string> ignoredFieldNames, ConnectionInfo connectionInfo, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(api);
            ArgumentNullException.ThrowIfNull(progress);
            ArgumentNullException.ThrowIfNull(connectionInfo);
            ArgumentNullException.ThrowIfNull(cancellationToken);

            if (api.SessionState != PsrSessionState.Connected)
            {
                throw new InvalidOperationException("PsrAPI is not authenticated.");
            }

            _api = api;
            _progress = progress;
            _ignoredFieldNames = ignoredFieldNames;
            _connectionInfo = connectionInfo;
            _cancellationToken = cancellationToken;

            _fieldsCount = 0;

            _knownPasswords = new Dictionary<string, List<string>>();
            _breachedPasswords = new Dictionary<string, int>();
            _checkExceptions = new Dictionary<string, Exception>();
        }

        #endregion Constructor

        #region Public methods

        public async Task Run()
        {
            _progress.StartTime = DateTime.Now;
            ThrowExceptionIfShouldCancel();

            UiThreadHelper.RunOnUiThread(() => _progress.Step = CheckerStep.LoadPasswordsCount);
            var passwordsCount = await GetNumberOfExistingPasswords();

            ThrowExceptionIfShouldCancel();

            UiThreadHelper.RunOnUiThread(() => _progress.Step = CheckerStep.LoadPasswords);
            var passwordFilter = CreatePasswordFilter();

            var passwords = new List<PsrContainer>();

            var pagesCount = (int) Math.Ceiling(passwordsCount / (double) PasswordsCountPerPage);
            for (var i = 0; i < pagesCount; i++)
            {
                ThrowExceptionIfShouldCancel();
                passwordFilter.Page = i;

                var passwordsPage = await GetPasswords(passwordFilter);
                passwords.AddRange(passwordsPage);
            }

            ThrowExceptionIfShouldCancel();

            UiThreadHelper.RunOnUiThread(() => _progress.Step = CheckerStep.CheckPasswords);
            await AnalyzePasswords(passwords);

            ThrowExceptionIfShouldCancel();

            UiThreadHelper.RunOnUiThread(() => _progress.Step = CheckerStep.PrepareReport);
            _progress.Report = await SummarizeReportData();

            UiThreadHelper.RunOnUiThread(() =>
            {
                _progress.IsFinished = true;
            });
        }

        #endregion Public methods

        #region Private methods

        private void ThrowExceptionIfShouldCancel()
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }
        }

        private PsrContainerListFilter CreatePasswordFilter()
        {
            var filter = new PsrContainerListFilter
            {
                PageSize = PasswordsCountPerPage,
                PageOrderAsc = true,
                PageOrder = nameof(PsrContainer.TimeStampUtc)
            };

            return filter;
        }

        private Task<int> GetNumberOfExistingPasswords()
        {
            return _api.ContainerManager.GetContainerCount(PsrContainerType.Password, new PsrContainerListFilter());
        }

        private Task<IEnumerable<PsrContainer>> GetPasswords(PsrContainerListFilter filter)
        {
            return _api.ContainerManager.GetContainerList(PsrContainerType.Password, filter);
        }

        private async Task AnalyzePasswords(List<PsrContainer> passwords)
        {
            foreach (var password in passwords)
            {
                ThrowExceptionIfShouldCancel();

                var itemsToCheck = password.Items?.Where(i => i.IsPasswordItem()).ToList();
                if (itemsToCheck == null || itemsToCheck.Count == 0)
                {
                    continue;
                }

                foreach (var item in itemsToCheck)
                {
                    ThrowExceptionIfShouldCancel();

                    var skip = false;
                    foreach (var ignore in _ignoredFieldNames)
                    {
                        try
                        {
                            if (Regex.IsMatch(item.DataName(), ignore))
                            {
                                skip = true;
                                break;
                            }
                        }
                        catch
                        {
                            // Ignore
                        }
                    }

                    if (skip)
                    {
                        _skippedFieldsCount++;
                        continue;
                    }

                    _fieldsCount++;

                    var itemIdentifier = $"{password.DataName()} -> {item.DataName()}";

                    try
                    {
                        await AnalyzeField(item, itemIdentifier);
                    }
                    catch (Exception ex)
                    {
                        while (_checkExceptions.ContainsKey(itemIdentifier))
                        {
                            itemIdentifier += "*";
                        }

                        _checkExceptions.Add(itemIdentifier, ex);
                    }
                    
                }
            }
        }

        private async Task AnalyzeField(PsrContainerItem item, string itemIdentifier)
        {
            var decryptedValue = await _api.ContainerManager.DecryptContainerItem(item, "Password Checker");
            if (string.IsNullOrWhiteSpace(decryptedValue))
            {
                return;
            }

            var quality = _api.PasswordManager.GetPasswordStrength(decryptedValue);
            StoreToQualityStore(itemIdentifier, quality);

            var hash = Sha1Helper.Hash(decryptedValue);
            _ = AddToKnownPasswords(hash, itemIdentifier);

            var foundInBreaches = await HaveIBeenPwned.CheckPassword(hash, true);
            if (foundInBreaches > 0)
            {
                while (_breachedPasswords.ContainsKey(itemIdentifier))
                {
                    itemIdentifier += "*";
                }

                _breachedPasswords.Add(itemIdentifier, foundInBreaches);
            }
        }

        private bool AddToKnownPasswords(string hash, string itemIdentifier)
        {
            if (_knownPasswords.TryGetValue(hash, out var password))
            {
                password.Add(itemIdentifier);
                return true;
            }

            _knownPasswords[hash] = [ itemIdentifier ];
            return false;
        }

        private void StoreToQualityStore(string itemIdentifier, int qualityPoints)
        {
            if (qualityPoints < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(qualityPoints), @"Invalid quality - less than 0 points is not allowed!");
            }

            if (qualityPoints < 30)
            {
                _qualityStore.WeakPasswords.Add(itemIdentifier);
            }
            else if (qualityPoints < 85)
            {
                _qualityStore.GoodPasswords.Add(itemIdentifier);
            }
            else
            {
                _qualityStore.StrongPasswords.Add(itemIdentifier);
            }
        }

        private Task<ReportData> SummarizeReportData()
        {
            return Task.Run(() =>
            {
                var leakedPasswords = _breachedPasswords.Select(p => new LeakedPassword(p.Key, p.Value));
                var quality = new QualityStore
                {
                    WeakPasswords = _qualityStore.WeakPasswords, 
                    GoodPasswords = _qualityStore.GoodPasswords,
                    StrongPasswords = _qualityStore.StrongPasswords
                };
                var duplicatedPasswords = _knownPasswords.Where(p => p.Value.Count > 1).Select(p => p.Value).ToArray();
                _connectionInfo.User = _api.CurrentUser.DataName();

                return new ReportData(_fieldsCount, _skippedFieldsCount, leakedPasswords.ToList(), quality,
                    duplicatedPasswords, _checkExceptions, _connectionInfo)
                {
                    CheckStart = _progress.StartTime,
                    CheckEnd = DateTime.Now
                };
            }, _cancellationToken);
        }

        #endregion Private methods

        #region IDisposable

        public void Dispose()
        {
            _breachedPasswords.Clear();
            _knownPasswords.Clear();

            _qualityStore.WeakPasswords.Clear();
            _qualityStore.GoodPasswords.Clear();
            _qualityStore.StrongPasswords.Clear();
        }

        #endregion IDisposable
    }
}
