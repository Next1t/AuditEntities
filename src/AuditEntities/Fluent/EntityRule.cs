using AuditEntities.Fluent.Abstractions;
using System.Linq.Expressions;

namespace AuditEntities.Fluent;

public abstract class EntityRule<TEntity, TPermission> : IEntityRule<TEntity, TPermission> where TEntity : class
{
    private readonly List<IPropertyRule<TEntity>> Rules = new();

    private TPermission? _permission;
    public TPermission? Permission => _permission;

    public void SetPermission(TPermission permission)
    {
        _permission = permission;
    }

    public IRuleBuilder<TEntity, TPermission, TProperty> RuleFor<TProperty>(Expression<Func<TEntity, TProperty>> expression)
    {
        var rule = PropertyRule<TEntity, TProperty>.Create(expression);
        Rules.Add(rule);
        return new RuleBulder<TEntity, TPermission, TProperty>(rule, this);
    }

    public virtual void ExecuteRules(string propertyName, object? value, Dictionary<string, object?> modifiedProperties)
    {
        var rules = Rules.Where(s => s.PropertyName.Equals(propertyName));

        if (!rules.Any())
        {
            modifiedProperties.Add(propertyName, value);
            return;
        }

        foreach (IPropertyRule<TEntity> rule in rules)
        {
            var result = rule.ExecuteRule(propertyName, value);
            if (result != null && result.Name != null)
            {
                modifiedProperties.Add(result.Name, result.Value);
            }
        }
    }
}

public abstract class EntityRule<TEntity, TPermission, TInstance> :
    EntityRule<TEntity, TPermission>, IEntityRule<TEntity, TPermission, TInstance>
    where TEntity : class
    where TInstance : class
{
}