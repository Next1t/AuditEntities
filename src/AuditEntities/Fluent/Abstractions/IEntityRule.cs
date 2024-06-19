namespace AuditEntities.Fluent.Abstractions;

public interface IEntityRule<TPermission>
{
    public TPermission? Permission { get; }
    void ExecuteRules(string propertyName, object? value, Dictionary<string, object> modifiedProperties);
}

public interface IEntityRule<TEntity, TPermission>
    : IEntityRule<TPermission>
{
}

public interface IEntityRule<TEntity, TPermission, TInstance> :
    IEntityRule<TEntity, TPermission>
{
}