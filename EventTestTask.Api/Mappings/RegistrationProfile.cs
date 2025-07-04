using AutoMapper;
using EventTestTask.Api.DTOs.Event;
using EventTestTask.Api.DTOs.Registration;
using EventTestTask.Api.DTOs.User;
using EventTestTask.Core.Entities;
using EventTestTask.Core.Models.Pagination;

namespace EventTestTask.Api.Mappings;

public class RegistrationProfile : Profile
{
    public RegistrationProfile()
    {
        CreateMap<Registration, RegistrationResponse>().ReverseMap();
        CreateMap<PageResult<Event>, PageResult<EventResponse>>().ReverseMap();
        CreateMap<PageResult<User>, PageResult<UserResponse>>().ReverseMap();
    }
}