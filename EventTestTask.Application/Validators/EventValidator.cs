using EventTestTask.Core.DTOs.Event;
using FluentValidation;

namespace EventTestTask.Application.Validators;

public class EventValidator : AbstractValidator<EventRequest>
{
    public EventValidator()
    {
        RuleFor(e => e.Title)
            .NotEmpty()
            .WithMessage("Title cannot be empty")
            .MaximumLength(100)
            .WithMessage("Title cannot exceed 50 characters");

        RuleFor(e => e.Description)
            .NotEmpty()
            .WithMessage("Description cannot be empty")
            .MaximumLength(200)
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(e => e.Location)
            .NotEmpty()
            .WithMessage("Location cannot be empty")
            .MaximumLength(100)
            .WithMessage("Location cannot exceed 100 characters");

        RuleFor(e => e.Category)
            .IsInEnum()
            .WithMessage("Category must be a valid event category");

        RuleFor(e => e.StartDate)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("Start date cannot be in the past")
            .LessThan(e => e.EndDate)
            .WithMessage("Start date must be before end date")
            .When(e => e.EndDate != default);

        RuleFor(e => e.EndDate)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("End date cannot be in the past")
            .GreaterThan(e => e.StartDate)
            .WithMessage("End date must be after start date")
            .When(e => e.StartDate != default);

        RuleFor(e => e.MaxParticipants)
            .GreaterThan(0)
            .WithMessage("Max participants must be greater than 0")
            .LessThanOrEqualTo(50)
            .WithMessage("Max participants cannot exceed 1000");

        RuleFor(e => e.Image)
            .Must(image => image is { Length: <= 5 * 1024 * 1024 })
            .WithMessage("Image size cannot exceed 5 MB");
    }
}