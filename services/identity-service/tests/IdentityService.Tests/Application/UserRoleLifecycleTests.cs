using IdentityService.Application.Abstractions;
using IdentityService.Application.Commands.CreateUser;
using IdentityService.Application.Commands.UpdateUser;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.Enums;
using IdentityService.Domain.Repositories;
using IdentityService.Domain.ValueObjects;
using Moq;

namespace IdentityService.Tests.Application;

public sealed class UserRoleLifecycleTests
{
    [Fact]
    public async Task NewMemberReceivesRoleBeforeChangesAreCommitted()
    {
        var tenant = Tenant.Register("Agency", new Email("owner@example.test"));
        var tenants = new Mock<ITenantRepository>();
        tenants.Setup(x => x.GetByIdAsync(tenant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        var users = new Mock<IUserRepository>(MockBehavior.Strict);
        var unit = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var sequence = new MockSequence();
        users.InSequence(sequence).Setup(x => x.GetByTenantAndEmailAsync(tenant.Id, "member@example.test", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        users.InSequence(sequence).Setup(x => x.AddAsync(It.Is<User>(u => u.Role == UserRole.Member), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        users.InSequence(sequence).Setup(x => x.SetSystemRoleAsync(It.Is<User>(u => u.Role == UserRole.Member && u.TenantId == tenant.Id), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        unit.InSequence(sequence).Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        await new CreateUserCommandHandler(tenants.Object, users.Object, unit.Object)
            .Handle(new CreateUserCommand(tenant.Id, "member@example.test", "TestPassword123!", "Member"), CancellationToken.None);
        users.VerifyAll();
        unit.VerifyAll();
    }

    [Fact]
    public async Task DemotionSynchronizesPermissionsWithMemberRole()
    {
        var user = User.Create(TenantId.New(), new Email("admin@example.test"), "hashed", UserRole.Admin);
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        await new UpdateUserCommandHandler(users.Object, Mock.Of<IUnitOfWork>())
            .Handle(new UpdateUserCommand(user.Id, "Member", null), CancellationToken.None);
        users.Verify(x => x.SetSystemRoleAsync(It.Is<User>(u => u.Role == UserRole.Member), It.IsAny<CancellationToken>()), Times.Once);
    }
}
