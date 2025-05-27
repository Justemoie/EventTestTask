using AutoMapper;
using EventTestTask.Api.DTOs.User;
using EventTestTask.Core.Entities;
using EventTestTask.Core.Enums;
using EventTestTask.Core.Models.Pagination;

namespace EventTestTask.Api.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserRequest>();
        CreateMap<UserResponse, User>().ReverseMap();
        CreateMap<PageResult<User>, PageResult<UserResponse>>().ReverseMap();
       
        CreateMap<UserRequest, User>()
            .ConstructUsing(src => new User(
                Guid.NewGuid(),
                src.FirstName,
                src.LastName,
                src.BirthDate,
                src.Email,
                src.PasswordHash,
                UserRole.User))
            .ForMember(dest => dest.Events, opt => opt.Ignore());
        
        CreateMap<RegisterUser, User>()
            .ConstructUsing(src => new User(
                Guid.NewGuid(),
                src.FirstName,
                src.LastName,
                src.BirthDate,
                src.Email,
                src.Password,
                UserRole.User))
            .ForMember(dest => dest.Events, opt => opt.Ignore());
    }
}