namespace AuditEntities.Models;

public record TrackedPropertiesWithPermission<TPermission>(TPermission? permission, IReadOnlyDictionary<string, object> trackedProperties)
{
    public TPermission? Permission { get; } = permission;
    public IReadOnlyDictionary<string, object> TrackedProperties { get; } = trackedProperties;
}

