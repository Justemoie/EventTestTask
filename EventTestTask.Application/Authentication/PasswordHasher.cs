using EventTestTask.Core.Interfaces.PasswordHasher;

namespace EventTestTask.Application.Authentication;

public class PasswordHasher : IPasswordHasher
{
    public string GenerateHash(string password) =>
        BCrypt.Net.BCrypt.EnhancedHashPassword(password);

    public bool VerifyHash(string password, string hash) =>
        BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
}