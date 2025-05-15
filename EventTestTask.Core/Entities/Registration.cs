namespace EventTestTask.Core.Entities;

public class Registration
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public User User { get; set; }
    
    public Guid EventId { get; private set; }

    public Event Event { get; set; }
    
    public DateTime RegistrationDate { get; private set; }

    public Registration(Guid id, Guid userId, Guid eventId, DateTime registrationDate)
    {
        Id = id;
        UserId = userId;
        EventId = eventId;
        RegistrationDate = registrationDate;
    }
}