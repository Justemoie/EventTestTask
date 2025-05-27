using EventTestTask.Core.Entities;
using FluentValidation;

namespace EventTestTask.Application.Validators;

public sealed class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        
        RuleFor(users => users.FirstName)
            .NotEmpty()
            .WithMessage("First name cannot be empty")
            .MaximumLength(50)
            .WithMessage("First name cannot exceed 50 characters");
        
        RuleFor(user => user.LastName)
            .NotEmpty()
            .WithMessage("Last name cannot be empty")
            .MaximumLength(50)
            .WithMessage("Last name cannot exceed 50 characters");

        RuleFor(e => e.BirthDate)
            .NotEmpty()
            .WithMessage("Birth date cannot be empty");
        
        RuleFor(user => user.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email address")
            .MaximumLength(100)
            .WithMessage("Email cannot exceed 50 characters");
    }
}