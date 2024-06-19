namespace AuditEntities.Fluent.Abstractions;

public interface IPropertyRule
{
    string PropertyName { get; }
    NameValue ExecuteRule(string name, object value);
}

public interface IPropertyRule<TEntity> : IPropertyRule
{
}

public interface IPropertyRule<TEntity, TProperty> : IPropertyRule<TEntity>
{
    public void Add(IPropertyRule<TEntity, TProperty> rule);
}