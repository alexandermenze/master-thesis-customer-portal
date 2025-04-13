namespace CustomerPortal.UserAuthService.Dtos;

public record SessionTokenResponseDto(Guid UserId, string Token, DateTimeOffset ExpiresAt);
