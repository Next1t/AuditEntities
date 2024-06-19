using static AuditEntities.Fluent.AssemblyScanner;

namespace AuditEntities.Fluent.Abstractions;

public interface IAuditEntitiesAssemblyProvider
{
    IEnumerable<AssemblyScanResult> AssemblyScanResult { get; }
}

public interface IAuditEntitiesAssemblyProvider<TInstance> : IAuditEntitiesAssemblyProvider
{

}
