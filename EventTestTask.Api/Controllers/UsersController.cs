using AutoMapper;
using EventTestTask.Api.DTOs.User;
using EventTestTask.Core.Entities;
using EventTestTask.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventTestTask.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;
    private readonly IMapper _mapper;
    
    public UsersController(IUsersService usersService, IMapper mapper)
    {
        _usersService = usersService;
        _mapper = mapper;
    }

    [HttpGet("{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetUserById([FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await _usersService.GetUserById(userId, cancellationToken);
        var userResponse = _mapper.Map<UserResponse>(user);
        return Ok(userResponse);
    }

    [HttpGet("email")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetUserByEmail([FromQuery] string email,
        CancellationToken cancellationToken)
    {
        var user = await _usersService.GetByEmail(email, cancellationToken);
        var userResponse = _mapper.Map<UserResponse>(user);
        return Ok(userResponse);
    }

    [HttpPut("update/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult> UpdateUser([FromRoute] Guid userId, [FromBody] UserRequest userRequest,
        CancellationToken cancellationToken)
    {
        var user = _mapper.Map<User>(userRequest);
        await _usersService.UpdateUser(userId, user, cancellationToken);
        return Ok();
    }
}