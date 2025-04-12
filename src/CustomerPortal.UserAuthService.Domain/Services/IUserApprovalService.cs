using CustomerPortal.UserAuthService.Domain.Aggregates;

namespace CustomerPortal.UserAuthService.Domain.Services;

public interface IUserApprovalService
{
    Task<User> Approve(Guid approverGuid, Guid candidateGuid);
}
