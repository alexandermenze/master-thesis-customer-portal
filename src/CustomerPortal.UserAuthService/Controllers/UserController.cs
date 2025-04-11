using CustomerPortal.UserAuthService.Domain.Aggregates;
using CustomerPortal.UserAuthService.Domain.DataClasses;
using CustomerPortal.UserAuthService.Domain.Repositories;
using CustomerPortal.UserAuthService.Domain.Services;
using CustomerPortal.UserAuthService.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CustomerPortal.UserAuthService.Controllers;

[ApiController]
[Route("users")]
public class UserController(
    IUserRepository userRepository,
    IRegisterUserService registerUserService
) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id)
    {
        var userResponse = UserResponseDto.From(await userRepository.GetById(id));

        if (userResponse is null)
            return NotFound();

        return Ok(userResponse);
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(User))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> Register([FromBody] RegisterUserData data)
    {
        var user = await registerUserService.Register(data);
        var userResponse = new UserResponseDto(user.Id, user.Email, user.FirstName, user.LastName);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, userResponse);
    }
}
