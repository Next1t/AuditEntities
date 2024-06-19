namespace AuditEntities.Models;
public record AuditEntitiesDataAfterSave<TPermission>
{
    public required Guid UniqueId { get; set; }
    public required object Entity { get; set; } = null!;
    public TPermission? RequiredReadPermission { get; set; }
    public required DateTime Timestamp { get; set; }
    public required AuditActionType Action { get; set; }
    public required string EntityName { get; set; } = null!;
    public string? EntityId { get; set; }
    public required string DataJson { get; set; } = null!;
    public required IReadOnlyDictionary<string, object> ModifiedProperties { get; set; } = null!;
}
