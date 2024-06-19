using AuditEntities.Fluent.Abstractions;

namespace AuditEntities.Fluent;
public class RuleBulder<TEntity, TPermission, TProperty> :
    IRuleBuilder<TEntity, TPermission, TProperty>
    where TEntity : class
{
    public IPropertyRule<TEntity, TProperty> Rule { get; }

    public EntityRule<TEntity, TPermission> Parent { get; }

    public RuleBulder(IPropertyRule<TEntity, TProperty> propertyRule, EntityRule<TEntity, TPermission> parent)
    {
        Rule = propertyRule;
        Parent = parent;
    }

    public IRuleBuilder<TEntity, TPermission, TProperty> SetRule(IPropertyRule<TEntity, TProperty> rule)
    {
        Rule.Add(rule);
        return this;
    }
}