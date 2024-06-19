using AuditEntities.Enums;
using AuditEntities.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace AuditEntities.Abstractions;

public interface IAuditEntitiesConsumer<TPermission>
{
    Task ConsumeAsync(IEnumerable<AuditEntitiesDataAfterSave<TPermission>> AuditEntitiesData, DbTransaction? transaction, TransactionEventData? eventData, CancellationToken cancellationToken = default);

    Task TransactionFinished(TransactionEventData dbContextEventData, TransactionStatus status, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

public interface IAuditEntitiesConsumer<TPermission, TInstance>
    : IAuditEntitiesConsumer<TPermission>
{
}