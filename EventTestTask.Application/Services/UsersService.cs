using System.Security.Authentication;
using EventTestTask.Core.Entities;
using EventTestTask.Core.Enums;
using EventTestTask.Core.Interfaces.PasswordHasher;
using EventTestTask.Core.Interfaces.Repositories;
using EventTestTask.Core.Interfaces.Services;
using EventTestTask.Core.Models.JWT;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace EventTestTask.Application.Services;

public class UsersService : IUsersService
{
    private readonly IValidator<User> _userValidator;
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokensService _jwtTokensService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsersService(IUsersRepository usersRepository, IValidator<User> userValidator,
        IPasswordHasher passwordHasher, IJwtTokensService jwtTokensService, IHttpContextAccessor httpContextAccessor)
    {
        _usersRepository = usersRepository;
        _userValidator = userValidator;
        _passwordHasher = passwordHasher;
        _jwtTokensService = jwtTokensService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<User> GetUserById(Guid userId, CancellationToken cancellationToken)
    {
        return await EnsureUserExists(userId, cancellationToken);
    }

    public async Task<User> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            throw new KeyNotFoundException("User not found");
        }

        return user;
    }

    public async Task Register(User user, CancellationToken cancellationToken)
    {
        var validationResult = await _userValidator.ValidateAsync(user, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var hashedPassword = _passwordHasher.GenerateHash(user.PasswordHash);
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

    public async Task UpdateUser(Guid userId, User user, CancellationToken cancellationToken)
    {
        await EnsureUserExists(userId, cancellationToken);
        await _userValidator.ValidateAndThrowAsync(user, cancellationToken);

        await _usersRepository.UpdateUserAsync(userId, user, cancellationToken);
    }

    public async Task<TokenResponse> Login(string email, string password, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null || !_passwordHasher.VerifyHash(password, user.PasswordHash))
        {
            throw new AuthenticationException("Invalid email or password");
        }

        var token = await _jwtTokensService.GenerateTokens(user, cancellationToken);

        _httpContextAccessor.HttpContext.Response.Cookies.Append("_at", token.AccessToken);
        _httpContextAccessor.HttpContext.Response.Cookies.Append("_rt", token.RefreshToken);

        return token;
    }

    public async Task Logout(CancellationToken cancellationToken)
    {
        var refreshToken = _httpContextAccessor.HttpContext.Request.Cookies["_rt"];
        await _jwtTokensService.InvalidateRefreshToken(refreshToken, cancellationToken);

        _httpContextAccessor.HttpContext.Response.Cookies.Delete("_at");
        _httpContextAccessor.HttpContext.Response.Cookies.Delete("_rt");
    }

    private async Task<User> EnsureUserExists(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new KeyNotFoundException("User not found");
        }

        return user;
    }
}