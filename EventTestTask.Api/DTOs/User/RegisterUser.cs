using System.ComponentModel.DataAnnotations;

namespace EventTestTask.Core.DTOs.User;

public record RegisterUser(
    [Required] string FirstName,
    [Required] string LastName,
    [Required] string Email,
    [Required] string Password,
    [Required] DateTime BirthDate
);