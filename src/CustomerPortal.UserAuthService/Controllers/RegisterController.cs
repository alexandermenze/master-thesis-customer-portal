using CustomerPortal.UserAuthService.Domain.Aggregates;
using CustomerPortal.UserAuthService.Domain.DataClasses;
using CustomerPortal.UserAuthService.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerPortal.UserAuthService.Controllers;

[ApiController]
[Route("[controller]")]
public class RegisterController(IRegisterUserService registerUserService) : ControllerBase
{
    [HttpPost]
    public Task<User> Register([FromBody] RegisterUserData data) =>
        registerUserService.Register(data);
}
