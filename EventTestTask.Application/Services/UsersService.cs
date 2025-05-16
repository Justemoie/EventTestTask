using AutoMapper;
using EventTestTask.Core.DTOs.User;
using EventTestTask.Core.Entities;
using EventTestTask.Core.Interfaces.Repositories;
using EventTestTask.Core.Interfaces.Services;
using FluentValidation;

namespace EventTestTask.Application.Services;

public class UsersService : IUsersService
{
    private readonly IValidator<UserRequest> _userValidator;
    private readonly IUsersRepository _usersRepository;
    private readonly IMapper _mapper;

    public UsersService(IUsersRepository usersRepository, IMapper mapper, IValidator<UserRequest> userValidator)
    {
        _usersRepository = usersRepository;
        _mapper = mapper;
        _userValidator = userValidator;
    }

    public async Task<UserResponse> GetUserById(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetUserByIdAsync(userId, cancellationToken);
        return _mapper.Map<UserResponse>(user);
    }

    public async Task<UserResponse> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetByEmailAsync(email, cancellationToken);
        return _mapper.Map<UserResponse>(user);
    }

    public async Task CreateUser(UserRequest user, CancellationToken cancellationToken)
    {
        await _userValidator.ValidateAndThrowAsync(user, cancellationToken);
        await _usersRepository.CreateUserAsync(_mapper.Map<User>(user), cancellationToken);
    }

    public async Task UpdateUser(Guid userId, UserRequest user, CancellationToken cancellationToken)
    {
        await _userValidator.ValidateAndThrowAsync(user, cancellationToken);
        await _usersRepository.UpdateUserAsync(userId, _mapper.Map<User>(user), cancellationToken);
    }
}