namespace AuditEntities.Fluent;
public record NameValue(string name, object? value)
{
    public string Name { get; } = name;
    public object? Value { get; } = value;
}
