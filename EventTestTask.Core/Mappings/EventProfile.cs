using AutoMapper;
using EventTestTask.Core.DTOs.Event;
using EventTestTask.Core.Entities;
using EventTestTask.Core.Models.Pagination;

namespace EventTestTask.Core.Mappings;

public class EventProfile : Profile
{
    public EventProfile()
    {
        CreateMap<EventRequest, Event>()
            .ConstructUsing(src => new Event(
                Guid.NewGuid(),
                src.Title,
                src.Description,
                src.StartDate,
                src.EndDate,
                src.Location,
                src.Category,
                src.MaxParticipants,
                src.Image ?? Array.Empty<byte>()))
            .ForMember(dest => dest.Participants, opt => opt.Ignore());

        CreateMap<Event, EventRequest>();

        CreateMap<EventResponse, Event>().ReverseMap();

        CreateMap<PageResult<Event>, PageResult<EventResponse>>().ReverseMap();
    }
}