using CustomerPortal.UserAuthService.Postgres.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerPortal.UserAuthService.Postgres;

public class UserAuthContext(DbContextOptions<UserAuthContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
}
