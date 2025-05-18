using EventTestTask.Core.DTOs.Jwt;
using EventTestTask.Core.DTOs.User;
using EventTestTask.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace EventTestTask.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokensService _tokensService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUsersService _usersService;

    public AuthController(IJwtTokensService tokensService, IHttpContextAccessor httpContextAccessor,
        IUsersService usersService)
    {
        _tokensService = tokensService;
        _httpContextAccessor = httpContextAccessor;
        _usersService = usersService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<string>> Register([FromBody] RegisterUser request,
        CancellationToken cancellationToken)
    {
        await _usersService.Register(request, cancellationToken);

        return Ok(new { Message = "Successfully registered" });
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginUser request,
        CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;
        
        if (context is null)
            return BadRequest();
        
        var token = await _usersService.Login(request.Email, request.Password, cancellationToken);
        
        context.Response.Cookies.Append("_at", token.AccessToken);
        context.Response.Cookies.Append("_rt", token.RefreshToken);
        
        return Ok(token);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["_rt"];
        
        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest(new { message = "Logout failed" });
        
        await _usersService.Logout(refreshToken, cancellationToken);
        
        Response.Cookies.Delete("_at", new CookieOptions
        {
            HttpOnly = true,
            Secure = HttpContext.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
        });
        Response.Cookies.Delete("_rt", new CookieOptions
        {
            HttpOnly = true,
            Secure = HttpContext.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
        });
        
        return Ok(new { message = "Successfully logged out" });
    }

    [HttpPost("update")]
    [Authorize]
    public async Task<IActionResult> UpdateTokens(CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;
        
        if (context is null)
            return BadRequest();
        
        var token = await _tokensService.UpdateTokens(context, cancellationToken);
        
        context.Response.Cookies.Append("_at", token.AccessToken);
        context.Response.Cookies.Append("_rt", token.RefreshToken);

        return Ok();
    }
}