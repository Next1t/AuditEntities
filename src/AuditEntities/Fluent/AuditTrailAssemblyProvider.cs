using AuditEntities.Fluent.Abstractions;
using static AuditEntities.Fluent.AssemblyScanner;

namespace AuditEntities.Fluent;
public class AuditEntitiesAssemblyProvider<TPermission>(IEnumerable<AssemblyScanner> assemblyScaners) : IAuditEntitiesAssemblyProvider<TPermission>
{
    public IEnumerable<AssemblyScanner> AssemblyScanners { get; } = assemblyScaners;
    public IEnumerable<AssemblyScanResult> AssemblyScanResult => AssemblyScanners.SelectMany(s => s);
}
