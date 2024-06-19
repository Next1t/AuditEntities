namespace AuditEntities.Options;

public class AuditEntitiesOptions
{
    /// <summary>
    /// Autmatically open transaction for savechanges if no transaction is open
    /// </summary>
    public bool AutoOpenTransaction { get; set; }
}

