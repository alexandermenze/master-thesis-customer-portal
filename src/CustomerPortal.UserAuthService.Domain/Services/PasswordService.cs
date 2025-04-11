using CustomerPortal.UserAuthService.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace CustomerPortal.UserAuthService.Domain.Services;

public class PasswordService(IPasswordHasher<string> passwordHasher) : IPasswordService
{
    public void EnsureRequirementsAreMet(string password)
    {
        if (password.Length < 12)
            throw new DomainValidationException("Password must be at least 12 characters.");
    }

    public string HashPassword(string email, string password) =>
        passwordHasher.HashPassword(email, password);
}
