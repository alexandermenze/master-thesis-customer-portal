using CustomerPortal.UserAuthService.Domain.Aggregates;

namespace CustomerPortal.UserAuthService.Dtos;

public record UserResponseDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool Approved
)
{
    public static UserResponseDto? From(User? user) =>
        user is null
            ? null
            : new UserResponseDto(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Approved
            );
}
