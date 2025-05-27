using AutoMapper;
using EventTestTask.Core.DTOs.Jwt;
using EventTestTask.Core.DTOs.User;
using EventTestTask.Core.Entities;
using EventTestTask.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventTestTask.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokensService _tokensService;
    private readonly IUsersService _usersService;
    private readonly IMapper _mapper;

    public AuthController(IJwtTokensService tokensService, IUsersService usersService, IMapper mapper)
    {
        _tokensService = tokensService;
        _usersService = usersService;
        _mapper = mapper;
    }

    [HttpPost("register")]
    public async Task<ActionResult<string>> Register([FromBody] RegisterUser request,
        CancellationToken cancellationToken)
    {
        var user = _mapper.Map<User>(request);
        await _usersService.Register(user, cancellationToken);
        return Ok(new { Message = "Successfully registered" });
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginUser request,
        CancellationToken cancellationToken)
    {
        var token = await _usersService.Login(request.Email, request.Password, cancellationToken);
        return Ok(token);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _usersService.Logout(cancellationToken);
        return Ok(new { message = "Successfully logged out" });
    }

    [HttpPost("update")]
    [Authorize]
    public async Task<IActionResult> UpdateTokens(CancellationToken cancellationToken)
    {
        await _tokensService.UpdateTokens(cancellationToken);
        return Ok();
    }
}