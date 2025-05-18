namespace EventTestTask.Core.Interfaces.PasswordHasher;

public interface IPasswordHasher
{
    string GenerateHash(string password);
    void VerifyHash(string password, string hash);
}