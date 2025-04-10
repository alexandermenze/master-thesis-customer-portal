using System.ComponentModel.DataAnnotations;

namespace CustomerPortal.UserAuthService.Postgres.Entities;

public class User
{
    public int Id { get; set; }

    [EmailAddress]
    [MaxLength(256)]
    public required string Email { get; set; }

    [MaxLength(256)]
    public required string Password { get; set; }
}
