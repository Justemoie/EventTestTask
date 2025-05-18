using System.ComponentModel.DataAnnotations;

namespace EventTestTask.Core.DTOs.User;

public record LoginUser(
    [Required] string Email,
    [Required] string Password
);