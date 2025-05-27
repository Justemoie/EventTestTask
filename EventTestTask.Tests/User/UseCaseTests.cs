using System.Security.Authentication;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using EventTestTask.Api.DTOs.User;
using EventTestTask.Application.Services;
using EventTestTask.Core.Enums;
using EventTestTask.Core.Interfaces.PasswordHasher;
using EventTestTask.Core.Interfaces.Repositories;
using EventTestTask.Core.Interfaces.Services;
using EventTestTask.Core.Models.JWT;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace EventTestTask.Tests.User;

public class UserRegistrationTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IUsersRepository> _usersRepository;
    private readonly Mock<IValidator<Core.Entities.User>> _userValidator;
    private readonly Mock<IPasswordHasher> _passwordHasher;
    private readonly Mock<IJwtTokensService> _jwtTokensService;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
    private readonly Mock<IMapper> _mapper;
    private readonly UsersService _usersService;

    public UserRegistrationTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _usersRepository = _fixture.Freeze<Mock<IUsersRepository>>();
        _userValidator = _fixture.Freeze<Mock<IValidator<Core.Entities.User>>>();
        _passwordHasher = _fixture.Freeze<Mock<IPasswordHasher>>();
        _jwtTokensService = _fixture.Freeze<Mock<IJwtTokensService>>();
        _httpContextAccessor = _fixture.Freeze<Mock<IHttpContextAccessor>>();
        _mapper = _fixture.Freeze<Mock<IMapper>>();

        _usersService = new UsersService(
            usersRepository: _usersRepository.Object,
            userValidator: _userValidator.Object,
            passwordHasher: _passwordHasher.Object,
            jwtTokensService: _jwtTokensService.Object,
            httpContextAccessor: _httpContextAccessor.Object
        );
    }

    [Fact]
    public async Task Register_WhenValidData_ShouldRegisterUser()
    {
        var registerUser = _fixture.Create<RegisterUser>();
        var userRequest = new Core.Entities.User(
            Guid.NewGuid(),
            registerUser.FirstName,
            registerUser.LastName,
            registerUser.BirthDate,
            registerUser.Email,
            registerUser.Password,
            UserRole.User
        );
        var hashedPassword = "hashed_password";

        _mapper
            .Setup(m => m.Map<Core.Entities.User>(registerUser))
            .Returns(userRequest);

        _userValidator
            .Setup(v => v.ValidateAsync(userRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _passwordHasher
            .Setup(h => h.GenerateHash(registerUser.Password))
            .Returns(hashedPassword);

        _usersRepository
            .Setup(r => r.CreateUserAsync(It.IsAny<Core.Entities.User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        await _usersService.Register(userRequest, CancellationToken.None);

        _usersRepository.Verify(
            x => x.CreateUserAsync(
                It.Is<Core.Entities.User>(u =>
                    u.Email == userRequest.Email &&
                    u.FirstName == userRequest.FirstName &&
                    u.LastName == userRequest.LastName &&
                    u.BirthDate == userRequest.BirthDate &&
                    u.PasswordHash == hashedPassword &&
                    u.Role == UserRole.User &&
                    u.Events.Count == 0),
                It.IsAny<CancellationToken>()),
            Times.Once());
    }

    [Fact]
    public async Task Register_WhenInvalidData_ShouldThrowValidationException()
    {
        var registerUser = new RegisterUser(
            FirstName: "User1",
            LastName: "User1",
            Email: "invalid_email",
            Password: "short",
            BirthDate: DateTime.Now.AddYears(-20)
        );

        var userRequest = _fixture.Create<Core.Entities.User>();

        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Email", "Invalid email format")
        });

        _mapper
            .Setup(m => m.Map<Core.Entities.User>(registerUser))
            .Returns(userRequest);

        _userValidator
            .Setup(v => v.ValidateAsync(userRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _usersService.Register(userRequest, CancellationToken.None));

        ex.Errors.Should().HaveCount(1);
        ex.Errors.Should().Contain(e =>
            e.ErrorMessage == "Invalid email format" &&
            e.PropertyName == "Email");

        _passwordHasher.Verify(h => h.GenerateHash(It.IsAny<string>()), Times.Never);
        _usersRepository.Verify(
            r => r.CreateUserAsync(It.IsAny<Core.Entities.User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        var email = "test@example.com";
        var password = "validPassword123";

        var user = new Core.Entities.User(
            id: Guid.NewGuid(),
            firstName: "Test",
            lastName: "User",
            birthDate: DateTime.Now.AddYears(-20),
            email: email,
            passwordHash: "hashed_password",
            role: UserRole.User
        );

        var expectedToken = _fixture.Create<TokenResponse>();
        
        var responseMock = new Mock<HttpResponse>();
        var responseCookiesMock = new Mock<IResponseCookies>();
        responseMock.Setup(r => r.Cookies).Returns(responseCookiesMock.Object);
        
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Response).Returns(responseMock.Object);

        _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContextMock.Object);

        _usersRepository
            .Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(h => h.VerifyHash(password, user.PasswordHash))
            .Returns(true);

        _jwtTokensService
            .Setup(j => j.GenerateTokens(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedToken);

        var result = await _usersService.Login(email, password, CancellationToken.None);

        result.Should().BeEquivalentTo(expectedToken);
        _passwordHasher.Verify(h => h.VerifyHash(password, user.PasswordHash), Times.Once);
        responseCookiesMock.Verify(c => c.Append("_at", expectedToken.AccessToken), Times.Once());
        responseCookiesMock.Verify(c => c.Append("_rt", expectedToken.RefreshToken), Times.Once());
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldThrowAuthenticationException()
    {
        var email = "test@example.com";
        var password = "wrongPassword";

        var user = new Core.Entities.User(
            id: Guid.NewGuid(),
            firstName: "Test",
            lastName: "User",
            birthDate: DateTime.Now.AddYears(-20), // Исправлено: дата в прошлом
            email: email,
            passwordHash: "hashed_password",
            role: UserRole.User
        );

        _usersRepository
            .Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(h => h.VerifyHash(password, user.PasswordHash))
            .Returns(false);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            _usersService.Login(email, password, CancellationToken.None));

        _usersRepository.Verify(ur => ur.GetByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasher.Verify(h => h.VerifyHash(password, user.PasswordHash), Times.Once);
        _jwtTokensService.Verify(jt => jt.GenerateTokens(user, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ShouldThrowKeyNotFoundException()
    {
        var email = "invalidEmail@example.com";
        var password = "password";

        _usersRepository
            .Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Core.Entities.User?)null);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            _usersService.Login(email, password, CancellationToken.None));

        _usersRepository.Verify(ur => ur.GetByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasher.Verify(h => h.VerifyHash(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Logout_WithValidToken_ShouldInvalidateToken()
    {
        var refreshToken = "validRefreshToken";

        var requestMock = new Mock<HttpRequest>();
        var cookiesMock = new Mock<IRequestCookieCollection>();
        cookiesMock.Setup(c => c["_rt"]).Returns(refreshToken);
        requestMock.Setup(r => r.Cookies).Returns(cookiesMock.Object);

        var responseMock = new Mock<HttpResponse>();
        var responseCookiesMock = new Mock<IResponseCookies>();
        responseMock.Setup(r => r.Cookies).Returns(responseCookiesMock.Object);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);
        httpContextMock.Setup(c => c.Response).Returns(responseMock.Object);

        _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContextMock.Object);
        _jwtTokensService
            .Setup(j => j.InvalidateRefreshToken(refreshToken, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _usersService.Logout(CancellationToken.None);

        _jwtTokensService.Verify(j => j.InvalidateRefreshToken(refreshToken, It.IsAny<CancellationToken>()),
            Times.Once());
        responseCookiesMock.Verify(c => c.Delete("_at"), Times.Once());
        responseCookiesMock.Verify(c => c.Delete("_rt"), Times.Once());
    }

    [Fact]
    public async Task Logout_WhenTokenServiceFails_ShouldThrowInvalidOperationException()
    {
        var refreshToken = "invalidToken";

        var requestMock = new Mock<HttpRequest>();
        var cookiesMock = new Mock<IRequestCookieCollection>();
        cookiesMock.Setup(c => c["_rt"]).Returns(refreshToken);
        requestMock.Setup(r => r.Cookies).Returns(cookiesMock.Object);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);

        _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContextMock.Object);
        _jwtTokensService
            .Setup(j => j.InvalidateRefreshToken(refreshToken, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Token invalid"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _usersService.Logout(CancellationToken.None));
    }
}