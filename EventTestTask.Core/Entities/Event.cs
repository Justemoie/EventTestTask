using EventTestTask.Core.Enums;

namespace EventTestTask.Core.Entities;

public class Event
{
    public Guid Id { get; set; }
    
    public Guid CreatorId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Location { get; set; }

    public EventCategory Category { get; set; }

    public int MaxParticipants { get; set; }

    public IList<Registration> Participants { get; set; } = new List<Registration>();

    public byte[]? Image { get; set; }

    public Event()
    {
        
    }
    
    public Event(Guid id, Guid creatorId, string title, string description, DateTime startDate, DateTime endDate, string location,
        EventCategory category, int maxParticipants, byte[]? image)
    {
        Id = id;
        CreatorId = creatorId;
        Title = title;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        Location = location;
        Category = category;
        MaxParticipants = maxParticipants;
        Image = image;
    }
}