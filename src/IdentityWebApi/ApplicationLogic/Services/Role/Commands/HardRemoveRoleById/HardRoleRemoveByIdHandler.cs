using IdentityWebApi.Core.Entities;
using IdentityWebApi.Core.Enums;
using IdentityWebApi.Core.Results;
using IdentityWebApi.Infrastructure.Database;

using MediatR;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityWebApi.ApplicationLogic.Services.Role.Commands.HardRemoveRoleById;

/// <summary>
/// Hard remove role by id CQRS handler.
/// </summary>
public class HardRemoveRoleByIdHandler : IRequestHandler<HardRemoveRoleByIdCommand, ServiceResult>
{
    private readonly DatabaseContext databaseContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="HardRemoveRoleByIdHandler"/> class.
    /// </summary>
    /// <param name="databaseContext">The instance of <see cref="DatabaseContext"/>.</param>
    public HardRemoveRoleByIdHandler(DatabaseContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    /// <inheritdoc />
    public async Task<ServiceResult> Handle(HardRemoveRoleByIdCommand request, CancellationToken cancellationToken)
    {
        var appRole = await this.GetAppRoleAsync(request.Id);

        if (appRole is null)
        {
            return new ServiceResult(ServiceResultType.NotFound);
        }

        await this.RemoveRoleAsync(appRole);

        return new ServiceResult(ServiceResultType.Success);
    }

    private async Task<AppRole> GetAppRoleAsync(Guid id) =>
        await this.databaseContext.SearchById<AppRole>(
            id,
            includeTracking: true,
            includedEntity => includedEntity.UserRoles);

    private async Task RemoveRoleAsync(AppRole appRole)
    {
        this.databaseContext.UserRoles.RemoveRange(appRole.UserRoles);
        this.databaseContext.Roles.Remove(appRole);

        await this.databaseContext.SaveChangesAsync();
    }
}
