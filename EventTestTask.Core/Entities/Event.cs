using EventTestTask.Core.Enums;

namespace EventTestTask.Core.Entities;

public class Event
{
    public Guid Id { get; private set; }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    public string Location { get; private set; }

    public EventCategory Category { get; private set; }

    public int MaxParticipants { get; private set; }

    public IList<Registration> Participants { get; private set; } = new List<Registration>();

    public byte[] Image { get; private set; }

    public Event(Guid id, string title, string description, DateTime startDate, DateTime endDate, string location,
        EventCategory category, int maxParticipants, byte[] image)
    {
        Id = id;
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