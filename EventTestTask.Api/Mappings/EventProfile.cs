using AutoMapper;
using EventTestTask.Api.DTOs.Event;
using EventTestTask.Core.Entities;
using EventTestTask.Core.Models.Pagination;

namespace EventTestTask.Api.Mappings;

public class EventProfile : Profile
{
    public EventProfile()
    {
        CreateMap<EventRequest, Event>()
            .ConstructUsing(src => new Event(
                Guid.NewGuid(),
                Guid.Empty, // CreatorId будет установлен в контроллере
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
        CreateMap<Event, EventResponse>().ReverseMap();
        CreateMap<EventResponse, Event>();
        CreateMap<PageResult<Event>, PageResult<EventResponse>>().ReverseMap();
    }
}