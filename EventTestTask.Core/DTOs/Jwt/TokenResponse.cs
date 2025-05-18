namespace EventTestTask.Core.DTOs.Jwt;

public record TokenResponse(
    DateTime Expiration,
    string RefreshToken,
    string AccessToken
);