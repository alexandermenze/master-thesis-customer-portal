using CustomerPortal.UserAuthService.Domain.Aggregates;
using CustomerPortal.UserAuthService.Domain.Exceptions;
using CustomerPortal.UserAuthService.Domain.Repositories;

namespace CustomerPortal.UserAuthService.Domain.Services;

public class UserManagementService(IUserRepository userRepository) : IUserManagementService
{
    public async Task<User> Approve(Guid approverGuid, Guid candidateGuid)
    {
        var approver = await userRepository.GetById(approverGuid);
        var candidate = await userRepository.GetById(candidateGuid);

        if (approver is null || candidate is null)
            throw new EntityNotFoundException("Current user or candidate not found.");

        if (AllowsStateChange(approver.Role, candidate.Role) is false)
            throw new DomainValidationException("Insufficient rights to approve.");

        candidate.Approve();
        await userRepository.Save(candidate);

        return candidate;
    }

    public async Task<User> Deactivate(Guid deactivatingPartyGuid, Guid candidateGuid)
    {
        var deactivatingParty = await userRepository.GetById(deactivatingPartyGuid);
        var candidate = await userRepository.GetById(candidateGuid);

        if (deactivatingParty is null || candidate is null)
            throw new EntityNotFoundException("Current user or candidate not found.");

        if (AllowsStateChange(deactivatingParty.Role, candidate.Role) is false)
            throw new DomainValidationException("Insufficient rights to approve.");

        candidate.Deactivate();
        await userRepository.Save(candidate);

        return candidate;
    }

    private static bool AllowsStateChange(UserRole stateChangerRole, UserRole candidateRole) =>
        (stateChangerRole, candidateRole) switch
        {
            (UserRole.SuperAdmin, _) => true,
            (UserRole.Admin, UserRole.SalesDepartment) => true,
            (UserRole.Admin, UserRole.Customer) => true,
            (UserRole.SalesDepartment, UserRole.Customer) => true,
            _ => false,
        };
}
