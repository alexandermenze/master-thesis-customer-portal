using System.Collections.Immutable;
using CustomerPortal.UserAuthService.Domain.Aggregates;
using CustomerPortal.UserAuthService.Domain.DataClasses;

namespace CustomerPortal.UserAuthService.Domain.Repositories;

public interface IUserRepository
{
    Task<ImmutableArray<User>> GetAll();
    Task<User?> GetById(Guid id);
    Task<User?> GetByEmail(string email);
    Task<User> Add(UserData userData);
}
