using BCrypt.Net;

namespace SenseCareAPI.Helpers;

public static class PasswordHelper
{
    public static string Hash(string plain) =>
        BCrypt.Net.BCrypt.HashPassword(plain);

    public static bool Verify(string plain, string hash) =>
        BCrypt.Net.BCrypt.Verify(plain, hash);
}
