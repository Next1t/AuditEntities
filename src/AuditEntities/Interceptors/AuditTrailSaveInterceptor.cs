using AuditEntities.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace AuditEntities.Interceptors;
public class AuditEntitiesSaveInterceptor<TPermission>(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SavingChangesStartedAsync(eventData);
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        await SavingChangesStartedAsync(eventData, cancellationToken);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        await FinishSaveChanges(eventData);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        FinishSaveChanges(eventData).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    public override void SaveChangesCanceled(DbContextEventData eventData)
    {
        ClearSaveData();
        base.SaveChangesCanceled(eventData);
    }

    public override Task SaveChangesCanceledAsync(DbContextEventData eventData, CancellationToken cancellationToken = default)
    {
        ClearSaveData();
        return base.SaveChangesCanceledAsync(eventData, cancellationToken);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        ClearSaveData();

        var AuditEntitiesService = GetAuditEntitiesService(httpContextAccessor);
        AuditEntitiesService?.SaveChangesFailedAsync(eventData).GetAwaiter().GetResult();
        base.SaveChangesFailed(eventData);
    }

    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        ClearSaveData();

        var AuditEntitiesService = GetAuditEntitiesService(httpContextAccessor);

        if (AuditEntitiesService != null)
        {
            await AuditEntitiesService.SaveChangesFailedAsync(eventData, cancellationToken);
        }

        await base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    protected virtual IAuditEntitiesService<TPermission>? GetAuditEntitiesService(IHttpContextAccessor httpContextAccessor)
    {
        return httpContextAccessor.HttpContext?.RequestServices?.GetService<IAuditEntitiesService<TPermission>>();
    }

    private Task SavingChangesStartedAsync(DbContextEventData eventData, CancellationToken cancellationToken = default)
    {
        var AuditEntitiesService = GetAuditEntitiesService(httpContextAccessor);

        if (AuditEntitiesService != null)
        {
            return AuditEntitiesService.SavingChangesStartedAsync(eventData, cancellationToken);
        }

        return Task.CompletedTask;
    }

    private Task FinishSaveChanges(DbContextEventData eventData, CancellationToken cancellationToken = default)
    {
        var AuditEntitiesService = GetAuditEntitiesService(httpContextAccessor);

        if (AuditEntitiesService != null)
        {
            return AuditEntitiesService.FinishSaveChanges(eventData);
        }

        return Task.CompletedTask;
    }

    private void ClearSaveData()
    {
        var AuditEntitiesService = GetAuditEntitiesService(httpContextAccessor);

        AuditEntitiesService?.ClearSaveData();
    }
}

public class AuditEntitiesSaveInterceptor<TPermission, TInstance>(IHttpContextAccessor httpContextAccessor)
    : AuditEntitiesSaveInterceptor<TPermission>(httpContextAccessor)
{
    protected override IAuditEntitiesService<TPermission>? GetAuditEntitiesService(IHttpContextAccessor httpContextAccessor)
    {
        return httpContextAccessor.HttpContext?.RequestServices?.GetService<IAuditEntitiesService<TPermission, TInstance>>();
    }
}
