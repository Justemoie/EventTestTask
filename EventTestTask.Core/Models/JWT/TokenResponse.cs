namespace EventTestTask.Core.Models.JWT;

public class TokenResponse
{
    public DateTime Expiration { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}