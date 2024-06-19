using AuditEntities.Enums;
using AuditEntities.Fluent.Abstractions;
using AuditEntities.Models;
using AuditEntities.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data.Common;
using System.Reflection;
using System.Security;

namespace AuditEntities.Abstractions;

public abstract class AuditEntitiesServiceBase<TPermission> : IAuditEntitiesService<TPermission>, IDisposable, IAsyncDisposable
{
    private readonly IAuditEntitiesConsumer<TPermission> _AuditEntitiesConsumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAuditEntitiesAssemblyProvider _auditAssemblyProvider;
    private readonly ILogger<AuditEntitiesServiceBase<TPermission>> _logger;
    private readonly AuditEntitiesOptions _options;

    private IDbContextTransaction? _transaction;
    private bool _transactionStarted = false;
    private bool _commitStarted = false;
    private bool _disposed = false;

    private readonly ConcurrentBag<AuditEntitiesDataAfterSave<TPermission>> _auditTransactionData = new();
    private readonly ConcurrentBag<AuditEntitiesDataBeforeSave<TPermission>> _AuditEntitiesSaveData = new();

    protected AuditEntitiesServiceBase(
        IAuditEntitiesConsumer<TPermission> AuditEntitiesConsumer,
        IServiceProvider serviceProvider,
        IAuditEntitiesAssemblyProvider auditAssemblyProvider,
        IOptions<AuditEntitiesOptions> options,
        ILogger<AuditEntitiesServiceBase<TPermission>> logger = null)
    {
        _AuditEntitiesConsumer = AuditEntitiesConsumer ?? throw new ArgumentNullException(nameof(AuditEntitiesConsumer));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _auditAssemblyProvider = auditAssemblyProvider ?? throw new ArgumentNullException(nameof(auditAssemblyProvider));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;

        LogDebug("AuditEntitiesServiceBase initialized");
    }

    protected virtual IEnumerable<EntityEntry?> GetChanges(ChangeTracker changeTracker)
    {
        var trackedEntityTypes = _auditAssemblyProvider.AssemblyScanResult
            .Select(s => s.InterfaceType.GetGenericArguments()[0])
            .ToList();

        return changeTracker.Entries()
            .Where(e => (e.State == EntityState.Modified || e.State == EntityState.Added || e.State == EntityState.Deleted)
                        && trackedEntityTypes.Contains(e.Entity.GetType()));
    }

    public async Task<IEnumerable<AuditEntitiesDataBeforeSave<TPermission>>> GetEntityTrackedPropertiesBeforeSave(
        DbContextEventData eventData, CancellationToken cancellationToken = default)
    {
        var auditEntities = new List<AuditEntitiesDataBeforeSave<TPermission>>();

        if (_auditAssemblyProvider.AssemblyScanResult is null || eventData.Context is null)
        {
            return auditEntities;
        }

        var changes = GetChanges(eventData.Context.ChangeTracker);

        foreach (var entityEntry in changes.Where(s => s?.Entity != null))
        {
            var auditEntity = entityEntry!.Entity;
            var entityProperties = entityEntry.State == EntityState.Added
                ? GetTrackedPropertiesWithValues(entityEntry.Properties, auditEntity)
                : GetTrackedPropertiesWithValues(entityEntry.Properties.Where(prop => prop.IsModified), auditEntity);

            var id = GetEntityId(auditEntity, eventData.Context.ChangeTracker);

            var auditData = new AuditEntitiesDataBeforeSave<TPermission>
            {
                Entity = entityEntry.Entity,
                RequiredReadPermission = entityProperties.Permission,
                EntityId = id,
                EntityName = entityEntry.GetType().GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? entityEntry.Entity.GetType().Name,
                Action = GetActionFromEntityState(entityEntry.State),
                DataJson = System.Text.Json.JsonSerializer.Serialize(entityProperties.TrackedProperties),
                Timestamp = DateTime.UtcNow,
                ModifiedProperties = entityProperties.TrackedProperties,
            };

            auditEntities.Add(auditData);
        }

        return auditEntities;
    }

    public IEnumerable<AuditEntitiesDataAfterSave<TPermission>> UpdateEntityPropertiesAfterSave(
        IEnumerable<AuditEntitiesDataBeforeSave<TPermission>> auditEntitiesData,
        DbContext context)
    {
        var auditEntitiesUpdatedData = new List<AuditEntitiesDataAfterSave<TPermission>>();
        foreach (var entityData in auditEntitiesData)
        {
            var entityId = entityData.EntityId;
            if (entityData.Action == AuditActionType.Create)
            {
                entityId = GetEntityId(entityData.Entity, context.ChangeTracker);
            }

            var auditModel = new AuditEntitiesDataAfterSave<TPermission>
            {
                UniqueId = entityData.UniqueId,
                Entity = entityData.Entity,
                RequiredReadPermission = entityData.RequiredReadPermission,
                EntityId = entityId,
                EntityName = entityData.EntityName,
                Action = entityData.Action,
                DataJson = entityData.DataJson,
                Timestamp = entityData.Timestamp,
                ModifiedProperties = entityData.ModifiedProperties,
            };
            auditEntitiesUpdatedData.Add(auditModel);
        }

        return auditEntitiesUpdatedData;
    }

    public async Task TransactionFinished(TransactionEventData eventData, TransactionStatus status, CancellationToken cancellationToken = default)
    {
        _transactionStarted = false;
        _commitStarted = false;

        if (_auditTransactionData.Any())
        {
            ClearTransactionData();
        }

        await _AuditEntitiesConsumer.TransactionFinished(eventData, status, cancellationToken);
    }

    public async Task FinishSaveChanges(DbContextEventData eventData, CancellationToken cancellationToken = default)
    {
        if (eventData?.Context != null && !_commitStarted)
        {
            var updatedData = UpdateEntityPropertiesAfterSave(_AuditEntitiesSaveData, eventData.Context);

            if (eventData.Context.Database.CurrentTransaction == null)
            {
                await SendToConsumerAsync(updatedData);
            }
            else
            {
                foreach (var data in updatedData)
                {
                    _auditTransactionData.Add(data);
                }

                if (_transactionStarted && _transaction != null)
                {
                    try
                    {
                        _commitStarted = true;
                        await _transaction.CommitAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        LogError("Commit transaction failed", ex);
                        await RollbackTransactionAsync();
                    }
                    finally
                    {
                        await DisposeTransactionAsync();
                    }
                }
            }

            ClearSaveData();
        }
    }

    public async Task SavingChangesStartedAsync(DbContextEventData eventData, CancellationToken cancellationToken = default)
    {
        if (_commitStarted)
        {
            ClearSaveData();
            ClearTransactionData();
            return;
        }

        if (eventData.Context != null)
        {
            if (_options.AutoOpenTransaction && eventData.Context.Database.CurrentTransaction == null)
            {
                _transaction = await eventData.Context.Database.BeginTransactionAsync(cancellationToken);
                _transactionStarted = true;
            }

            var auditData = await GetEntityTrackedPropertiesBeforeSave(eventData, cancellationToken);
            foreach (var data in auditData)
            {
                _AuditEntitiesSaveData.Add(data);
            }
        }
    }

    public async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        if (_options.AutoOpenTransaction && _transaction != null)
        {
            await RollbackTransactionAsync();
        }
    }

    public Task BeforeTransactionCommitedAsync(DbTransaction transaction, TransactionEventData eventData, CancellationToken cancellationToken = default)
    {
        if (eventData.Context?.Database.CurrentTransaction != null
            && _auditTransactionData.Any(s => s.Action == AuditActionType.Update && s.ModifiedProperties.Any() || s.Action != AuditActionType.Update))
        {
            return _AuditEntitiesConsumer.ConsumeAsync(_auditTransactionData, transaction, eventData, cancellationToken);
        }

        return Task.CompletedTask;
    }

    public void ClearTransactionData()
    {
        _auditTransactionData.Clear();
    }

    public void ClearSaveData()
    {
        _AuditEntitiesSaveData.Clear();
    }

    protected string? GetEntityId(object entity, ChangeTracker changeTracker)
    {
        return changeTracker.Entries()
            .FirstOrDefault(e => e.Entity == entity)?
            .Properties
            .FirstOrDefault(p => p.Metadata.IsPrimaryKey())?
            .CurrentValue?
            .ToString();
    }

    protected TrackedPropertiesWithPermission<TPermission> GetTrackedPropertiesWithValues(
        IEnumerable<PropertyEntry> properties, object entity)
    {
        var modifiedProperties = new Dictionary<string, object>();
        var ruleService = GetService(entity.GetType(), typeof(TPermission)) as IEntityRule<TPermission>;

        if (ruleService == null) throw new ArgumentNullException($"Missing service for rule {entity.GetType().FullName}");

        var permission = ruleService.Permission;

        foreach (var property in properties)
        {
            if (property.Metadata?.PropertyInfo is null || (property.Metadata.IsPrimaryKey() && property.Metadata.ValueGenerated != ValueGenerated.OnAdd))
            {
                continue;
            }

            ruleService.ExecuteRules(property.Metadata.PropertyInfo.Name, property.CurrentValue, modifiedProperties);
        }

        return new TrackedPropertiesWithPermission<TPermission>(permission, modifiedProperties.AsReadOnly());
    }

    protected AuditActionType GetActionFromEntityState(EntityState entityState)
    {
        return entityState switch
        {
            EntityState.Added => AuditActionType.Create,
            EntityState.Modified => AuditActionType.Update,
            EntityState.Deleted => AuditActionType.Delete,
            _ => throw new SecurityException("Invalid Entity State"),
        };
    }

    protected virtual object GetService(params Type[] types)
    {
        var openGenericType = typeof(IEntityRule<,>);
        var closedGenericType = openGenericType.MakeGenericType(types);

        var entityRule = _serviceProvider.GetService(closedGenericType);
        if (entityRule == null)
        {
            throw new ArgumentNullException($"Missing service for rule {closedGenericType.FullName}");
        }

        return entityRule;
    }

    protected void LogDebug(string logMessage)
    {
        _logger?.LogDebug(logMessage);
    }

    protected void LogError(string message, Exception exception)
    {
        _logger?.LogError(exception, message);
    }

    private async Task SendToConsumerAsync(IEnumerable<AuditEntitiesDataAfterSave<TPermission>> AuditEntitiesData, CancellationToken cancellationToken = default)
    {
        try
        {
            if (AuditEntitiesData.Any(s => s.Action == AuditActionType.Update && s.ModifiedProperties.Any() || s.Action != AuditActionType.Update))
            {
                await _AuditEntitiesConsumer.ConsumeAsync(AuditEntitiesData, null, null, cancellationToken);
                ClearTransactionData();
            }
        }
        catch (Exception ex)
        {
            LogError("Send to audit trail consumer failed", ex);
        }
    }

    private async Task RollbackTransactionAsync()
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
            }
        }
        catch (Exception ex)
        {
            LogError("Rollback transaction failed", ex);
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        _commitStarted = false;
        _transactionStarted = false;
    }

    private void DisposeTransaction()
    {
        if (_transaction != null)
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            DisposeTransaction();
        }

        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
            return;

        await DisposeTransactionAsync();
        _disposed = true;
    }

    ~AuditEntitiesServiceBase()
    {
        Dispose(false);
    }
}
