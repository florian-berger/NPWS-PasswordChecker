namespace PasswordChecker.Data
{
    public enum CheckerStep
    {
        NotStarted = -1,
        ConnectAndLogin = 0,
        LoadPasswordsCount = 1,
        LoadPasswords = 2,
        CheckPasswords = 3,
        CreateReport = 4,
        Finish = 5
    }
}
