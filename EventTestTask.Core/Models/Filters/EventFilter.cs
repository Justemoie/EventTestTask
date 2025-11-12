using EventTestTask.Core.Enums;

namespace EventTestTask.Core.Models.Filters;

public class EventFilter
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; }
    public EventCategory? Category { get; set; }
    public string? SearchTerm { get; set; }
}