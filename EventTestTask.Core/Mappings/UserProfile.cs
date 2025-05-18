using AutoMapper;
using EventTestTask.Core.DTOs.User;
using EventTestTask.Core.Entities;
using EventTestTask.Core.Enums;
using EventTestTask.Core.Models.Pagination;

namespace EventTestTask.Core.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
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

        CreateMap<User, UserRequest>();

        CreateMap<UserResponse, User>().ReverseMap();

        CreateMap<PageResult<User>, PageResult<UserResponse>>().ReverseMap();
        
        CreateMap<RegisterUser, UserRequest>()
            .ConstructUsing(src => new UserRequest(
                src.FirstName,
                src.LastName,
                src.BirthDate,
                src.Email,
                src.Password));
        
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