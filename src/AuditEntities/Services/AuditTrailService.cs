using AuditEntities.Abstractions;
using AuditEntities.Fluent.Abstractions;
using AuditEntities.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuditEntities.Services;

public sealed class AuditEntitiesService<TPermission>(
    IAuditEntitiesConsumer<TPermission> audtTrailConsumer,
    IServiceProvider serviceProvider,
    IAuditEntitiesAssemblyProvider<TPermission> auditAssemblyProvider,
    IOptions<AuditEntitiesOptions> options,
    ILogger<AuditEntitiesService<TPermission>> logger = null) :
    AuditEntitiesServiceBase<TPermission>(audtTrailConsumer, serviceProvider, auditAssemblyProvider, options, logger)
{
}

public sealed class AuditEntitiesService<TPermission, TInstance>(
    IAuditEntitiesConsumer<TPermission, TInstance> audtTrailConsumer,
    IServiceProvider serviceProvider,
    IAuditEntitiesAssemblyProvider<TInstance> auditAssemblyProvider,
    IOptions<AuditEntitiesOptions> options,
    ILogger<AuditEntitiesService<TPermission>> logger) :
    AuditEntitiesServiceBase<TPermission>(audtTrailConsumer, serviceProvider, auditAssemblyProvider, options, logger),
    IAuditEntitiesService<TPermission, TInstance>
{
    protected override object GetService(params Type[] types)
    {
        var openGenericType = typeof(IEntityRule<,,>);
        var requiredTypes = types.ToList();
        requiredTypes.Add(typeof(TInstance));
        var closedGenericType = openGenericType.MakeGenericType(requiredTypes.ToArray());

        var entityRule = serviceProvider.GetService(closedGenericType);

        if (entityRule is null)
        {
            throw new ArgumentNullException($"Missing service for rule {closedGenericType.FullName}");
        }

        return entityRule;
    }
}