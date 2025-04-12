using System.Collections.Immutable;
using CustomerPortal.UserAuthService.Domain.Aggregates;
using CustomerPortal.UserAuthService.Domain.DataClasses;
using CustomerPortal.UserAuthService.Domain.Factories;
using CustomerPortal.UserAuthService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CustomerPortal.UserAuthService.Postgres.Repositories;

public class DbContextUserRepository(UserAuthContext userAuthContext, IUserFactory userFactory)
    : IUserRepository
{
    public async Task<ImmutableArray<User>> GetAll() => [.. await QueryUsers.ToListAsync()];

    public async Task<ImmutableArray<User>> GetByRole(UserRole role) =>
        [.. await QueryUsers.Where(u => u.Role == role).ToListAsync()];

    public Task<User?> GetById(Guid id) => QueryUsers.FirstOrDefaultAsync(x => x.Id == id);

    public Task<User?> GetByEmail(string email) =>
        QueryUsers.FirstOrDefaultAsync(x => x.Email == email);

    public async Task<User> Add(UserData userData)
    {
        var user = userFactory.Create(userData);
        userAuthContext.Users.Add(user);
        await userAuthContext.SaveChangesAsync();
        return user;
    }

    public async Task<User> Save(User user)
    {
        if (userAuthContext.Users.Entry(user).State is EntityState.Detached)
            throw new InvalidOperationException("User must be retrieved from the repository.");

        await userAuthContext.SaveChangesAsync();
        return user;
    }

    public async Task Delete(Guid id)
    {
        var user = await userAuthContext.Users.FindAsync(id);

        if (user is null)
            return;

        userAuthContext.Users.Remove(user);
        await userAuthContext.SaveChangesAsync();
    }

    private IQueryable<User> QueryUsers => userAuthContext.Users.Include(u => u.SessionTokens);
}
