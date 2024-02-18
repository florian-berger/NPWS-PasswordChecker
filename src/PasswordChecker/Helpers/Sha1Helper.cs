using System.Security.Cryptography;
using System.Text;

namespace PasswordChecker.Helpers
{
    internal static class Sha1Helper
    {
        internal static string Hash(string input)
        {
            var hash = SHA1.HashData(Encoding.UTF8.GetBytes(input));

            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
            {
                // can be "x2" if you want lowercase
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
