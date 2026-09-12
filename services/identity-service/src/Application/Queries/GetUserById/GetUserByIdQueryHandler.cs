using Dapper;
using IdentityService.Application.Abstractions;
using IdentityService.Application.ReadModels;
using IdentityService.Domain.Exceptions;
using MediatR;

namespace IdentityService.Application.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<GetUserByIdQuery, UserReadModel>
{
    public async Task<UserReadModel> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        const string sql = """
            SELECT "Id" AS id,
                   "TenantId" AS TenantId,
                   "Email" AS email,
                   "Role"::text AS role,
                   "Status"::text AS status,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt,
                   last_login_at AS LastLoginAt,
                   password_changed_at AS PasswordChangedAt,
                   must_change_password AS MustChangePassword
            FROM users
            WHERE "Id" = @UserId AND deleted_at IS NULL;
            """;

        var user = await connection.QuerySingleOrDefaultAsync<UserReadModel>(new CommandDefinition(sql, new { request.UserId }, cancellationToken: cancellationToken));
        return user ?? throw new NotFoundException("User not found.");
    }
}
