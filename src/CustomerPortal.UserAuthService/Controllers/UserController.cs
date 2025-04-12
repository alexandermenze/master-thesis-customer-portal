using System.Security.Claims;
using CustomerPortal.UserAuthService.Domain.DataClasses;
using CustomerPortal.UserAuthService.Domain.Exceptions;
using CustomerPortal.UserAuthService.Domain.Repositories;
using CustomerPortal.UserAuthService.Domain.Services;
using CustomerPortal.UserAuthService.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerPortal.UserAuthService.Controllers;

[ApiController]
[Route("users")]
public class UserController(
    IUserRepository userRepository,
    IRegisterUserService registerUserService,
    ILoginUserService loginUserService,
    IUserApprovalService userApprovalService
) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserResponseDto>))]
    public async Task<IActionResult> GetUsers()
    {
        var users = await userRepository.GetAll();
        var userResponseDtos = users.Select(UserResponseDto.From);
        return Ok(userResponseDtos);
    }

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
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserResponseDto))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> Register([FromBody] RegisterUserData data)
    {
        var user = await registerUserService.Register(data);
        var userResponse = UserResponseDto.From(user);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, userResponse);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SessionTokenResponseDto))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto data)
    {
        var result = await loginUserService.Login(data.Email, data.Password);

        if (result.HasValue is false)
            throw new EntityNotFoundException("User not found.");

        var (user, sessionToken) = result.Value;

        return Ok(new SessionTokenResponseDto(user.Id, sessionToken.Token, sessionToken.ExpiresAt));
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> Approve(Guid id)
    {
        var currentUserGuidString = HttpContext
            .User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
            ?.Value;

        if (Guid.TryParse(currentUserGuidString, out var currentUserGuid) is false)
            throw new EntityNotFoundException("Current user not found.");

        return Ok(UserResponseDto.From(await userApprovalService.Approve(currentUserGuid, id)));
    }
}
