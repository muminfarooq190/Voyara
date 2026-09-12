using IdentityService.Domain.Aggregates;
using IdentityService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(IdentityDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await dbContext.Users.SingleOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    public async Task<User?> GetByTenantAndEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken)
        => await dbContext.Users.SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Email == email && x.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<User>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken)
        => await dbContext.Users.Where(x => x.TenantId == tenantId && x.DeletedAt == null).ToListAsync(cancellationToken);

    public Task AddAsync(User user, CancellationToken cancellationToken)
        => dbContext.Users.AddAsync(user, cancellationToken).AsTask();

    public Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        dbContext.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task SetSystemRoleAsync(User user, CancellationToken cancellationToken)
    {
        var role = await dbContext.RoleDefinitions.SingleAsync(
            x => x.TenantId == null && x.NormalizedName == user.Role.ToString().ToUpperInvariant(), cancellationToken);
        var systemRoleIds = dbContext.RoleDefinitions.Where(x => x.TenantId == null).Select(x => x.Id);
        var previous = await dbContext.UserRoleAssignments.Where(x =>
            x.TenantId == user.TenantId && x.UserId == user.Id && systemRoleIds.Contains(x.RoleDefinitionId))
            .ToListAsync(cancellationToken);
        dbContext.UserRoleAssignments.RemoveRange(previous.Where(x => x.RoleDefinitionId != role.Id));
        if (!previous.Any(x => x.RoleDefinitionId == role.Id))
            dbContext.UserRoleAssignments.Add(UserRoleAssignment.Create(user.TenantId, user.Id, role.Id));
    }
}
