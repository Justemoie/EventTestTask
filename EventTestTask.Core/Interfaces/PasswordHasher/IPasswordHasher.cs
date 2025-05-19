namespace EventTestTask.Core.Interfaces.PasswordHasher;

public interface IPasswordHasher
{
    string GenerateHash(string password);
    bool VerifyHash(string password, string hash);
}