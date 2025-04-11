using CustomerPortal.UserAuthService.Domain.Aggregates;
using CustomerPortal.UserAuthService.Domain.DataClasses;
using CustomerPortal.UserAuthService.Domain.Repositories;

namespace CustomerPortal.UserAuthService.Domain.Services;

public class RegisterUserService(IPasswordService passwordService, IUserRepository userRepository)
    : IRegisterUserService
{
    public Task<User> Register(RegisterUserData data)
    {
        passwordService.EnsureRequirementsAreMet(data.Password);

        var userData = new UserData(
            data.Email,
            passwordService.HashPassword(data.Email, data.Password),
            data.FirstName,
            data.LastName
        );

        return userRepository.Add(userData);
    }
}
