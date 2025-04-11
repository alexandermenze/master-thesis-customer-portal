using CustomerPortal.UserAuthService.Domain.Factories;
using CustomerPortal.UserAuthService.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerPortal.UserAuthService.Domain.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddUserAuthService(this IServiceCollection services)
    {
        services.AddTransient<IPasswordService, PasswordService>();
        services.AddTransient<IRegisterUserService, RegisterUserService>();
        services.AddTransient<IUserFactory, UserFactory>();
    }
}
