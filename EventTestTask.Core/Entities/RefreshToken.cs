namespace EventTestTask.Core.Entities;

public class RefreshToken
{
    public Guid UserId { get; set; }
    
    public User? User { get; set; }
    
    public string Token { get; set; } = string.Empty;
    
    public DateTime Expiration { get; set; }
}