using AuditEntities.Enums;
using AuditEntities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace AuditEntities.Abstractions;
public interface IAuditEntitiesService<TPermission>
{
    Task BeforeTransactionCommitedAsync(DbTransaction transaction, TransactionEventData eventData, CancellationToken cancellationToken = default);
    Task TransactionFinished(TransactionEventData eventData, TransactionStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditEntitiesDataBeforeSave<TPermission>>> GetEntityTrackedPropertiesBeforeSave(DbContextEventData eventData, CancellationToken cancellationToken = default);
    IEnumerable<AuditEntitiesDataAfterSave<TPermission>> UpdateEntityPropertiesAfterSave(IEnumerable<AuditEntitiesDataBeforeSave<TPermission>> auditEntitiesData,
        DbContext context);
    Task FinishSaveChanges(DbContextEventData eventData, CancellationToken cancellationToken = default);
    Task SavingChangesStartedAsync(DbContextEventData eventData, CancellationToken cancellationToken = default);
    Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default);
    void ClearTransactionData();
    void ClearSaveData();
}

public interface IAuditEntitiesService<TPermission, TInstance>
    : IAuditEntitiesService<TPermission>
{
}
