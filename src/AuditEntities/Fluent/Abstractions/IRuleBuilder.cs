namespace AuditEntities.Fluent.Abstractions;
public interface IRuleBuilder<TEntity, TPermission, TProperty>
    where TEntity : class
{
    IRuleBuilder<TEntity, TPermission, TProperty> SetRule(IPropertyRule<TEntity, TProperty> rule);
}