using EventTestTask.Core.DTOs.User;
using EventTestTask.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventTestTask.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpGet("{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetUserById([FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await _usersService.GetUserById(userId, cancellationToken);

        return Ok(user);
    }

    [HttpGet("/email")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetByEmail([FromQuery] string email,
        CancellationToken cancellationToken)
    {
        var user = await _usersService.GetByEmail(email, cancellationToken);

        return Ok(user);
    }

    [HttpPut("/update/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult> UpdateUser([FromRoute] Guid userId, [FromBody] UserRequest userRequest,
        CancellationToken cancellationToken)
    {
        await _usersService.UpdateUser(userId, userRequest, cancellationToken);
        
        return Ok();
    }
}