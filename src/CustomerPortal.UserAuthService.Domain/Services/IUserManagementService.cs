using CustomerPortal.UserAuthService.Domain.Aggregates;

namespace CustomerPortal.UserAuthService.Domain.Services;

public interface IUserManagementService
{
    Task<User> Approve(Guid approverGuid, Guid candidateGuid);
    Task<User> Deactivate(Guid deactivatingPartyGuid, Guid candidateGuid);
}
