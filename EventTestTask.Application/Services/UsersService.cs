using System.Security.Authentication;
using AutoMapper;
using EventTestTask.Core.DTOs.Jwt;
using EventTestTask.Core.DTOs.User;
using EventTestTask.Core.Entities;
using EventTestTask.Core.Enums;
using EventTestTask.Core.Interfaces.PasswordHasher;
using EventTestTask.Core.Interfaces.Repositories;
using EventTestTask.Core.Interfaces.Services;
using FluentValidation;

namespace EventTestTask.Application.Services;

public class UsersService : IUsersService
{
    private readonly IValidator<UserRequest> _userValidator;
    private readonly IUsersRepository _usersRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokensService _jwtTokensService;

    public UsersService(IUsersRepository usersRepository, IMapper mapper, IValidator<UserRequest> userValidator,
        IPasswordHasher passwordHasher, IJwtTokensService jwtTokensService)
    {
        _usersRepository = usersRepository;
        _mapper = mapper;
        _userValidator = userValidator;
        _passwordHasher = passwordHasher;
        _jwtTokensService = jwtTokensService;
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

    public async Task Register(RegisterUser user, CancellationToken cancellationToken)
    {
        await _userValidator.ValidateAndThrowAsync(_mapper.Map<UserRequest>(user), cancellationToken);

        var hashedPassword = _passwordHasher.GenerateHash(user.Password);

        var newUser = new User(
            Guid.NewGuid(),
            user.FirstName,
            user.LastName,
            user.BirthDate,
            user.Email,
            hashedPassword,
            UserRole.User
        );

        await _usersRepository.CreateUserAsync(newUser, cancellationToken);
    }

    public async Task UpdateUser(Guid userId, UserRequest userRequest, CancellationToken cancellationToken)
    {
        await _userValidator.ValidateAndThrowAsync(userRequest, cancellationToken);
        await _usersRepository.UpdateUserAsync(userId, _mapper.Map<User>(userRequest), cancellationToken);
    }

    public async Task<TokenResponse> Login(string email, string password, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetByEmailAsync(email, cancellationToken);

        _passwordHasher.VerifyHash(password, user.PasswordHash);

        var token = await _jwtTokensService.GenerateTokens(user, cancellationToken);

        return token;
    }

    public async Task Logout(string refreshToken, CancellationToken cancellationToken) =>
        await _jwtTokensService.InvalidateRefreshToken(refreshToken, cancellationToken);
}